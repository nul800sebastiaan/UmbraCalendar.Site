using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire.Console;
using Hangfire.Server;
using UmbraCalendar.Database;
using UmbraCalendar.Meetup.Models;
using UmbraCalendar.Meetup.Models.Areas;
using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.AuthorizedServices.Services;

namespace UmbraCalendar.Meetup;

public class MeetupService : IMeetupService
{
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;
    private readonly IDatabaseService _databaseService;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public MeetupService(IAuthorizedServiceCaller authorizedServiceCaller,
	    IDatabaseService databaseService,
	    IWebHostEnvironment hostingEnvironment)
    {
	    _authorizedServiceCaller = authorizedServiceCaller;
	    _databaseService = databaseService;
	    _hostingEnvironment = hostingEnvironment;
    }

    public void GetUpcomingMeetupEvents(PerformContext context)
    {
	    var events= _databaseService.GetUpcomingMeetupEvents().Result;
	    context.WriteLine($"{events.Count} events are in the cache");
    }
    
    public void GetMeetupGroups(PerformContext context)
    {
	    var groups = _databaseService.GetMeetupGroups().Result;
		context.WriteLine($"{groups.Count} groups are in the cache");
    }
    
    public async Task ImportUpcomingMeetupEvents(PerformContext context)
    {
        context.WriteLine("Starting");

        var eventDataQuery = GetEventDataQuery();
        
        var query = $$"""
                      query($urlname: ID!) {
                      	 proNetwork(urlname: $urlname) {
                      	   eventsSearch(input: { first: 100, filter: { status: "UPCOMING" } }) {
                      	     {{eventDataQuery}}
                      	   }
                      	 }
                      }
                      """;

        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = new { urlname = "umbraco" }
        };
        
        // var responseRaw = _authorizedServiceCaller.SendRequestRawAsync(
	       //  "meetup",
	       //  "/gql-ext",
	       //  HttpMethod.Post,
	       //  requestContent
        // ).Result;
        //
        // var res = responseRaw;

        // Can't await this one because the context.Writeline doesn't work then
        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, MeetupEvents>(
	        "meetup",
	        "/gql-ext",
	        HttpMethod.Post,
	        requestContent
        ).Result;

        if (response.Success && response.Result != null)
        {
	        foreach (var edge in response.Result.Data?.Data.MeetupNetwork.EventsSearch.Edges)
	        {
		        var meetupEvent = edge.MeetupEvent;
		        context.WriteLine(
			        $"Meetup group {meetupEvent.Group.UrlName} has an event in {meetupEvent.Venue?.Name ?? "[online?]"} on {meetupEvent.StartDateLocal} titled {meetupEvent.Title} - more info: {meetupEvent.EventUrl}");
		        await FetchAllRsvpsForEvent(meetupEvent, context);
		        await _databaseService.UpsertItemAsync(meetupEvent, Constants.MeetupEventsContainerId);
	        }
        }
        else
        {
	        context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
        }
    }

    public async Task ImportMeetupEventsForGroup(PerformContext context, string urlName, string type = "upcoming")
    {
        context.WriteLine("Starting");

        var eventDataQuery = GetEventDataQuery();
        
        var eventFilter = type == "upcoming" 
            ? "ACTIVE" 
            : "PAST";
            
        var query = $$"""
                    query($urlname: String!) {
                    	 groupByUrlname(urlname: $urlname) {
                    	   events(first: 100, status: {{eventFilter}}) {
                            {{eventDataQuery}}
                    	   }
                    	 }
                    }
                    """;
        
        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = new { urlname = urlName }
        };
        
        // var responseRaw = _authorizedServiceCaller.SendRequestRawAsync(
	       //  "meetup",
	       //  "/gql-ext",
	       //  HttpMethod.Post,
	       //  requestContent
        // ).Result;
        //
        // var res = responseRaw;

        // Can't await this one because the context.Writeline doesn't work then
        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, MeetupEvents>(
	        "meetup",
	        "/gql-ext",
	        HttpMethod.Post,
	        requestContent
        ).Result;

        // if response.Result.Data.GroupByUrlname is null, the group doesn't exist (any more)
        if (response.Success && response.Result != null && response.Result.Data != null && response.Result.Data.Data.GroupByUrlname != null)
        {
	        foreach (var edge in response.Result.Data.Data.GroupByUrlname.Events.Edges)
	        {
		        var meetupEvent = edge.MeetupEvent;
		        context.WriteLine(
			        $"Meetup group {meetupEvent.Group.UrlName} has an event in {meetupEvent.Venue?.Name ?? "[online?]"} on {meetupEvent.StartDateLocal} titled {meetupEvent.Title} - more info: {meetupEvent.EventUrl}");
		        await FetchAllRsvpsForEvent(meetupEvent, context);
		        await _databaseService.UpsertItemAsync(meetupEvent, Constants.MeetupEventsContainerId);
	        }
        }
        else
        {
	        context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
        }
    }

    private static string GetEventDataQuery()
    {
	    const string eventDataQuery = """
	                              totalCount
	                              pageInfo { endCursor hasNextPage }
	                              edges {
	                                node {
	                                  id
	                             	 title
	                             	 dateTime
	                             	 duration
	                             	 endTime
	                             	 eventUrl
	                             	 description
	                             	 eventType
	                             	 venues { name address city state postalCode country lat lon }
	                             	 group { keyGroupPhoto { id baseUrl } country city urlname name timezone }
	                             	 featuredEventPhoto { id baseUrl }
	                             	 rsvps(first: 100) {
	                             	   pageInfo {
	                             	     hasNextPage
	                             	     endCursor
	                             	   }
	                             	   edges {
	                             	     node {
	                             	       id
	                             	       member {
	                             	         id
	                             	         name
	                             	         city
	                             	         state
	                             	         zip
	                             	         country
	                             	         bio
	                             	         memberPhoto {
	                             	           baseUrl
	                             	         }
	                             	       }
	                             	     }
	                             	   }
	                             	 }
	                                }
	                              }
	                             """;
	    return eventDataQuery;
    }

    private async Task FetchAllRsvpsForEvent(MeetupEvent meetupEvent, PerformContext context)
    {
	    if (meetupEvent.Rsvps?.PageInfo?.HasNextPage != true)
	    {
		    return; // No more pages to fetch
	    }

	    var allRsvps = new List<RsvpEdge>(meetupEvent.Rsvps.Edges);
	    var endCursor = meetupEvent.Rsvps.PageInfo.EndCursor;

	    while (endCursor != null)
	    {
		    const string query = """
		                         query($eventId: ID!, $cursor: String) {
		                           event(id: $eventId) {
		                             rsvps(first: 100, after: $cursor) {
		                               pageInfo {
		                                 hasNextPage
		                                 endCursor
		                               }
		                               edges {
		                                 node {
		                                   id
		                                   member {
		                                     id
		                                     name
		                                     city
		                                     state
		                                     zip
		                                     country
		                                     bio
		                                     memberPhoto {
		                                       baseUrl
		                                     }
		                                   }
		                                 }
		                               }
		                             }
		                           }
		                         }
		                         """;

		    var requestContent = new MeetupRequest
		    {
			    Query = query,
			    Variables = new { eventId = meetupEvent.id, cursor = endCursor }
		    };

		    var response = await _authorizedServiceCaller.SendRequestAsync<MeetupRequest, EventRsvpsResponse>(
			    "meetup",
			    "/gql-ext",
			    HttpMethod.Post,
			    requestContent
		    );

		    if (response.Success && response.Result != null)
		    {
			    var result = response.Result;
			    var eventData = result.Data;
			    if (eventData?.DataWrapper?.EventWrapper?.Rsvps != null)
			    {
				    var rsvps = eventData.DataWrapper.EventWrapper.Rsvps;
				    allRsvps.AddRange(rsvps.Edges);

				    if (rsvps.PageInfo.HasNextPage)
				    {
					    endCursor = rsvps.PageInfo.EndCursor;
				    }
				    else
				    {
					    endCursor = null; // Exit the loop
				    }
			    }
		    }
		    else
		    {
			    context.WriteLine($"Failed to fetch additional RSVPs for event {meetupEvent.Title}: {response.Exception?.Message}");
			    break;
		    }
	    }

	    meetupEvent.Rsvps.Edges = allRsvps;
	    context.WriteLine($"Fetched total of {allRsvps.Count} RSVPs for event {meetupEvent.Title}");
    }

    public async Task ImportHistoricMeetupEvents(PerformContext context)
    {
	    var hasNextPage = true;
	    var endCursor = "null";
	    while (hasNextPage)
	    {
		    var response = GetMeetupEvents(context, endCursor);
		    if(response == null)
		    {
			    context.WriteLine("Response for event search was null, no events found");
			    return;
		    }

		    foreach (var edge in response.Edges)
		    {
			    var meetupEvent = edge.MeetupEvent;
			    context.WriteLine($"Meetup group {meetupEvent.Group.UrlName} has an event in {meetupEvent.Venue?.Name ?? "[online?]"} on {meetupEvent.StartDateLocal} titled {meetupEvent.Title} - more info: {meetupEvent.EventUrl}");
			    await FetchAllRsvpsForEvent(meetupEvent, context);
			    await _databaseService.UpsertItemAsync(meetupEvent, Constants.MeetupEventsContainerId);
		    }

		    hasNextPage = response.PageInfo.HasNextPage;
		    endCursor = response.PageInfo.EndCursor;
	    }
    }

    private EventsSearch? GetMeetupEvents(PerformContext context, string? endCursor)
    {
	    var eventDataQuery = GetEventDataQuery();
	    var query = $$"""
	                  query($urlname: ID!, $cursor: String) {
	                  	 proNetwork(urlname: $urlname) {
	                  	   eventsSearch(input: { first: 100, after: $cursor, filter: { status: "PAST" } }) {
	                  	     {{eventDataQuery}}
	                  	   }
	                  	 }
	                  }
	                  """;

	    var requestContent = new MeetupRequest
	    {
		    Query = query,
		    Variables = new { urlname = "umbraco", cursor = endCursor }
	    };


	    // var responseRaw = _authorizedServiceCaller.SendRequestRawAsync(
	    //  "meetup",
	    //  "/gql-ext",
	    //  HttpMethod.Post,
	    //  requestContent
	    // ).Result;
	    //
	    // var res = responseRaw;

	    // Can't await this one because the context.Writeline doesn't work then
	    var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, MeetupEvents>(
		    "meetup",
		    "/gql-ext",
		    HttpMethod.Post,
		    requestContent
	    ).Result;

	    if (response.Success && response.Result != null)
	    {
		    return response.Result.Data.Data.MeetupNetwork.EventsSearch;
	    }
	    else
	    {
		    context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
		    return null;
	    }
    }
    public async Task ImportMeetupGroup(PerformContext context, string urlName)
    {
        const string query = """
                             query ($urlname: String!) {
                               groupByUrlname(urlname: $urlname) {
                                 id
                                 keyGroupPhoto { id baseUrl }
                                 name
                                 urlname
                                 timezone
                                 link
                                 stats { memberCounts { all } }
                                 groupAnalytics { totalMembers totalPastEvents }
                               }
                             }
                             """;

        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = new { urlname = urlName }
        };

        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, Groups>(
            "meetup",
            "/gql-ext",
            HttpMethod.Post,
            requestContent
        ).Result;

        // if response.Result.Data.MeetupGroup is null then the group used to exist but not any more
        if (response.Success && response.Result != null && response.Result.Data != null && response.Result.Data.Data.MeetupGroup != null)
        {
	        var existingGroups = _databaseService.GetMeetupGroups().Result;
	        var group = response.Result.Data.Data.MeetupGroup;
	        var existingGroup = existingGroups.FirstOrDefault(x => x.id == group.id);
	        // Preserve added metadata
	        if (existingGroup != null && !string.IsNullOrWhiteSpace(existingGroup.Area))
	        {
		        group.Area = existingGroup.Area;
	        }
	        group.GroupAnalytics = new GroupAnalytics { TotalMembers = group.Stats.MemberCounts.All, TotalPastEvents = group.GroupAnalytics != null ? group.GroupAnalytics.TotalPastEvents : 0 };
	        await _databaseService.UpsertItemAsync(group, Constants.MeetupGroupsContainerId);
            context.WriteLine($"Successfully imported group: {group.Name}");
        }
        else
        {
            context.WriteLine($"Failed to import group. Error: {response.Exception?.Message}");
        }
    }
    
    public async Task ImportNetworkGroups(PerformContext context)
    {
        const string query = """
                             query ($urlname: ID!) {
                               proNetwork(urlname: $urlname) {
                                 groupsSearch(input: {first: 100}) {
                                   totalCount
                                   pageInfo { endCursor hasNextPage }
                                   edges {
                                     node {
                                       id
                                       keyGroupPhoto { id baseUrl }
                                       name
                                       urlname
                                       timezone
                                       link
                                       stats { memberCounts { all } }
                                       groupAnalytics { totalMembers totalPastEvents }
                                     }
                                   }
                                 }
                               }
                             }
                             """;

        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = new { urlname = "umbraco" }
        };
	      
        // Can't await this one because the context.Writeline doesn't work then
        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, Groups>(
	        "meetup",
	        "/gql-ext",
	        HttpMethod.Post,
	        requestContent
        ).Result;

        if (response.Success && response.Result != null)
        {
	        var existingGroups = _databaseService.GetMeetupGroups().Result;
			foreach (var edge in response.Result.Data.Data.ProNetwork.GroupsSearch.Edges)
	        {
		        var group = edge.MeetupGroup;
		        context.WriteLine($"Processing Meetup group {group.Name} with {group.GroupAnalytics.TotalMembers} members and {group.GroupAnalytics.TotalPastEvents} past events");
		        var existingGroup = existingGroups.FirstOrDefault(x => x.id == group.id);
		        
		        // Preserve added metadata
		        if (existingGroup != null && !string.IsNullOrWhiteSpace(existingGroup.Area))
		        {
			        group.Area = existingGroup.Area;
		        }
		        await _databaseService.UpsertItemAsync(group, Constants.MeetupGroupsContainerId);
	        }
        }
        else
        {
	        context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
        }
    }

    public async Task TryForceRefreshToken(PerformContext context)
    {
	    var rawResponse = "";
	    try
	    {
		    const string query = """
	                           query ($urlname: ID!) {  proNetwork (urlname: $urlname) {  description} }
	                           """;

		    var requestContent = new MeetupRequest
		    {
			    Query = query,
			    Variables = new { urlname = "umbraco" }
		    };
	      
		    var response = await _authorizedServiceCaller.SendRequestAsync<MeetupRequest, object>(
			    "meetup",
			    "/gql-ext",
			    HttpMethod.Post,
			    requestContent
		    );
		    
		    rawResponse = response.Result?.RawResponse;
		    if (response.Success)
		    {
			    var token = await _authorizedServiceCaller.GetOAuth2AccessToken("meetup");
			    var tokenString = token.Result;
			    var handler = new JwtSecurityTokenHandler();
			    var jwt = handler.ReadJwtToken(tokenString);
			    var expires = jwt.ValidTo.ToLocalTime().ToString("s"); 
			    context.WriteLine($"Token refresh attempt succeeded. Token expires {expires}");
		    }
	    }
		catch (Exception e)
	    {
		    context.WriteLine($"Token refresh check got raw response: {rawResponse}");
	    }
    }
    
    public async Task<List<MeetupArea>> GetAvailableAreas()
    {
	    var webRoot = _hostingEnvironment.WebRootPath;
	    var areasConfig = webRoot + "/Config/MeetupAreas.json";
	    await using var openStream = File.OpenRead(areasConfig);
	    var areas = await JsonSerializer.DeserializeAsync<List<MeetupArea>>(openStream);
	    return areas;
    }
}

public class MeetupRequest
{
    [JsonPropertyName("query")] public string Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")] public object? Variables { get; set; } = null;

    [JsonPropertyName("operationName")] public string? OperationName { get; set; } = null;
}

public class EventRsvpsResponse
{
    [JsonPropertyName("data")]
    public EventRsvpsDataWrapper DataWrapper { get; set; }
}

public class EventRsvpsDataWrapper
{
    [JsonPropertyName("event")]
    public EventWithRsvpsWrapper EventWrapper { get; set; }
}

public class EventWithRsvpsWrapper
{
    [JsonPropertyName("rsvps")]
    public Models.Events.Rsvps Rsvps { get; set; }
}