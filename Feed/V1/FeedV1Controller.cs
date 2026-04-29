using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using UmbraCalendar.Database;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Feed.V1;

[ApiController]
[Route("api/v1/feed.json")]
public class FeedV1Controller : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IServiceProvider _serviceProvider;

    public FeedV1Controller(
        IDatabaseService databaseService,
        IUmbracoContextFactory umbracoContextFactory,
        IServiceProvider serviceProvider)
    {
        _databaseService = databaseService;
        _umbracoContextFactory = umbracoContextFactory;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var events = new List<FeedEvent>();

        using var _ = _umbracoContextFactory.EnsureUmbracoContext();
        using var serviceScope = _serviceProvider.CreateScope();
        var query = serviceScope.ServiceProvider.GetRequiredService<IPublishedContentQuery>();
        var rootNode = query.ContentAtRoot().FirstOrDefault();
        var eventsNode = rootNode?.Children().OfType<Events>();

        if (eventsNode != null)
        {
            foreach (var node in eventsNode)
            {
                foreach (var umbracoEvent in node.Descendants().OfType<CalendarEvent>()
                             .Where(x => x.DateTo >= DateTime.Today))
                {
                    if (umbracoEvent.EventLink?.Url == null) continue;

                    var (startsAt, endsAt) = ToOffsets(
                        umbracoEvent.DateFrom, umbracoEvent.DateTo, umbracoEvent.TimezoneIdentifier);

                    events.Add(new FeedEvent
                    {
                        Id = umbracoEvent.Key.ToString(),
                        Url = umbracoEvent.EventLink.Url,
                        Title = umbracoEvent.Name,
                        Summary = umbracoEvent.Description?.ToString(),
                        PublishedAt = new DateTimeOffset(umbracoEvent.UpdateDate, TimeSpan.Zero),
                        StartsAt = startsAt,
                        EndsAt = endsAt,
                        Location = string.IsNullOrWhiteSpace(umbracoEvent.EventLocation)
                            ? null
                            : umbracoEvent.EventLocation,
                        Organizer = umbracoEvent.EventHost,
                        AttendanceMode = ManualAttendanceMode(
                            umbracoEvent.EventLocation, umbracoEvent.OnlineAttendance),
                        IsHqOrganized = umbracoEvent.HqOrganizedEvent,
                        IsCancelled = false,
                    });
                }
            }
        }

        var meetupEvents = await _databaseService.GetUpcomingMeetupEvents();
        foreach (var meetupEvent in meetupEvents)
        {
            events.Add(new FeedEvent
            {
                Id = meetupEvent.id,
                Url = meetupEvent.EventUrl,
                Title = meetupEvent.Title,
                Summary = meetupEvent.Description,
                PublishedAt = !string.IsNullOrEmpty(meetupEvent.CreatedTime)
                    ? DateTimeOffset.Parse(meetupEvent.CreatedTime, CultureInfo.InvariantCulture)
                    : DateTimeOffset.UtcNow,
                StartsAt = DateTimeOffset.Parse(meetupEvent.StartDateTime, CultureInfo.InvariantCulture),
                EndsAt = DateTimeOffset.Parse(meetupEvent.EndDateTime, CultureInfo.InvariantCulture),
                Location = FormatVenue(meetupEvent),
                Organizer = meetupEvent.Group?.Name,
                AttendanceMode = MeetupAttendanceMode(meetupEvent.EventType),
                IsHqOrganized = false,
                IsCancelled = meetupEvent.IsCancelled,
            });
        }

        var ordered = events.OrderBy(e => e.StartsAt).ToList();

        var document = new FeedDocument
        {
            Feed = new FeedMeta
            {
                Title = "Umbraco events on Meetup.com",
                SourceUrl = "https://umbracalendar.com/",
                GeneratedAt = DateTimeOffset.UtcNow,
            },
            Events = ordered,
        };

        return Ok(document);
    }

    private static (DateTimeOffset startsAt, DateTimeOffset endsAt) ToOffsets(
        DateTime from, DateTime to, string? timezoneId)
    {
        TimeZoneInfo tz;
        try
        {
            tz = string.IsNullOrWhiteSpace(timezoneId)
                ? TimeZoneInfo.Local
                : TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.Local;
        }

        var fromUnspecified = DateTime.SpecifyKind(from, DateTimeKind.Unspecified);
        var toUnspecified = DateTime.SpecifyKind(to, DateTimeKind.Unspecified);
        return (
            new DateTimeOffset(fromUnspecified, tz.GetUtcOffset(fromUnspecified)),
            new DateTimeOffset(toUnspecified, tz.GetUtcOffset(toUnspecified))
        );
    }

    private static string ManualAttendanceMode(string? eventLocation, bool onlineAttendance)
    {
        switch (eventLocation)
        {
            case "Online event":
                return "online";
            case "Hybrid event":
                return "hybrid";
            default:
                var hasLocation = !string.IsNullOrWhiteSpace(eventLocation);
                if (onlineAttendance)
                {
                    return hasLocation ? "hybrid" : "online";
                }
                return "inPerson";
        }
    }

    private static string MeetupAttendanceMode(string? eventType) => eventType switch
    {
        "ONLINE" => "online",
        "HYBRID" => "hybrid",
        _ => "inPerson",
    };

    private static string? FormatVenue(Meetup.Models.Events.MeetupEvent meetupEvent)
    {
        if (meetupEvent.OnlineVenue != null) return null;
        if (meetupEvent.Venue == null) return null;
        var parts = new[]
        {
            meetupEvent.Venue.Name,
            meetupEvent.Venue.Address,
            meetupEvent.Venue.City,
            meetupEvent.Venue.Country,
        };
        var joined = string.Join(", ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}
