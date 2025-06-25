using UmbraCalendar.Calendar.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup;

public interface IUpcomingMeetupService
{
    Task<List<CalendarItem>> GetUpcomingMeetupEvents(List<CalendarEvent>? upcomingEvents);
}