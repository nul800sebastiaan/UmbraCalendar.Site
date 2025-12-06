using System.Text.Json.Serialization;
using UmbraCalendar.Meetup.Models;

namespace UmbraCalendar.Meetup.Models.Events;

public class MeetupEvent
{
    // Not annotated since CosmosDb really likes lower case `id`
    public string id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("dateTime")]
    public string StartDateTime { get; set; }
    public string StartTimeLocal => StartDateTime.Split("T").Skip(1).First().Substring(0, 5);
    public string StartDateLocal => StartDateTime.Split("T").First();
    [JsonPropertyName("duration")]
    public string Duration { get; set; }
    [JsonPropertyName("endTime")]
    public string EndDateTime { get; set; }
    public string EndTimeLocal => EndDateTime.Split("T").Skip(1).First().Substring(0, 5);
    public string EndDateLocal => EndDateTime.Split("T").First();
    [JsonPropertyName("createdTime")]
    public string CreatedTime { get; set; }
    [JsonPropertyName("eventUrl")]
    public string EventUrl { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    // going and isOnline fields no longer available in API
    [JsonPropertyName("group")]
    public Group Group { get; set; }
    [JsonPropertyName("eventType")]
    // ONLINE | PHYSICAL | HYBRID
    public string EventType { get; set; }
    [JsonPropertyName("venues")]
    public List<Venue>? Venues { get; set; }
    
    // Backward compatibility properties
    [JsonIgnore]
    public Venue? Venue => Venues?.FirstOrDefault();
    
    [JsonIgnore]
    public OnlineVenue? OnlineVenue => EventType == "ONLINE" ? new OnlineVenue { Type = "ONLINE" } : null;
    
    [JsonIgnore]
    public bool IsOnline => EventType == "ONLINE" || EventType == "HYBRID";
    
    [JsonIgnore]
    public int Going => Rsvps?.Edges?.Count ?? 0;
    [JsonPropertyName("featuredEventPhoto")]
    public Image? FeaturedEventPhoto { get; set; }
    
    [JsonPropertyName("rsvps")]
    public Rsvps Rsvps { get; set; }
}

public class Rsvps
{
    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }

    [JsonPropertyName("edges")]
    public List<RsvpEdge> Edges { get; set; }
}