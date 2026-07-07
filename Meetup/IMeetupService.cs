using Hangfire.Server;
using UmbraCalendar.Meetup.Models.Areas;

namespace UmbraCalendar.Meetup;

public interface IMeetupService
{
    Task ImportUpcomingMeetupEvents(PerformContext context);
    void GetUpcomingMeetupEvents(PerformContext context);
    Task ImportNetworkGroups(PerformContext context);
    void GetMeetupGroups(PerformContext context);
    Task ImportHistoricMeetupEvents(PerformContext context);
    Task<List<MeetupArea>> GetAvailableAreas();
    Task ImportMeetupGroup(PerformContext context, string urlName);
    Task ImportMeetupEventsForGroup(PerformContext context, string urlName, string type = "upcoming");
    Task ImportAdHocMeetupEvents(PerformContext context);
    Task ImportSingleMeetupEvent(PerformContext context, string eventIdOrUrl);
    Task TryForceRefreshToken(PerformContext context);
}