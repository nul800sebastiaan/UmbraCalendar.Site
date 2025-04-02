using Hangfire.Server;
using UmbraCalendar.Meetup.Models.Areas;

namespace UmbraCalendar.Meetup;

public interface IMeetupService
{
    void ImportUpcomingMeetupEvents(PerformContext context);
    void GetUpcomingMeetupEvents(PerformContext context);
    void ImportNetworkGroups(PerformContext context);
    void GetMeetupGroups(PerformContext context);
    void ImportHistoricMeetupEvents(PerformContext context);
    Task<List<MeetupArea>> GetAvailableAreas();
    void ImportMeetupGroup(PerformContext context, string urlName);
    void ImportMeetupEventsForGroup(PerformContext context, string urlName, string type = "upcoming");
}