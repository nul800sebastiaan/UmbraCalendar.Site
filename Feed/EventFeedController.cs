using Microsoft.AspNetCore.Mvc;
using UmbraCalendar.Meetup;

namespace UmbraCalendar.Feed;


[ApiController]
[Route("api/[controller]")]
public class EventFeedController : ControllerBase
{
    private readonly IUpcomingMeetupService _upcomingMeetupService;

    public EventFeedController(IUpcomingMeetupService upcomingMeetupService)
    {
        _upcomingMeetupService = upcomingMeetupService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var events = await _upcomingMeetupService.GetUpcomingMeetupEvents(null);
        var feedItems = new List<EventFeedModel>();
        foreach (var eventItem in events)
        {
            var feedItem = new EventFeedModel
            {
                Link = eventItem.Url,
                Date = eventItem.DateTimeFrom,
                Name = eventItem.Title,
                Organizer = eventItem.Organizer,
                ImageUrl = eventItem.Banner,
                HqOrganizedEvent = eventItem.HqOrganizedEvent
            };
            feedItems.Add(feedItem);
        }
        
        return Ok(feedItems);
    }
}