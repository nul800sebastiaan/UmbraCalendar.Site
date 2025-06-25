using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;

namespace UmbraCalendar.Database;

public interface IDatabaseService
{
    Task<bool> UpsertItemAsync<T>(T item, string collectionName);
    Task<List<MeetupEvent>> GetUpcomingMeetupEvents();
    Task<List<MeetupGroup>> GetMeetupGroups();
    Task<List<MeetupEvent>> GetAllMeetupEvents(DateTime startDate);
}