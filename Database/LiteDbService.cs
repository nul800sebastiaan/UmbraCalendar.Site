using LiteDB;
using Microsoft.Extensions.Options;
using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace UmbraCalendar.Database;

public class LiteDbService : IDatabaseService
{
    private readonly string _databasePath;
    private readonly IAppPolicyCache _runtimeCache;
    private const int CacheTimespanShort = 5;
    private const int CacheTimespanMedium = 60;
    private const int CacheTimespanLong = 60 * 4;
    
    public LiteDbService(
        IOptions<Options> config,
        IAppPolicyCache runtimeCache)
    {
        _runtimeCache = runtimeCache;
        _databasePath = config.Value.DatabasePath ?? "Data/UmbraCalendar.db";
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<bool> UpsertItemAsync<T>(T item, string collectionName)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<T>(collectionName);
            
            // Try to get the id property using reflection for upsert logic
            var idProperty = typeof(T).GetProperty("id") ?? typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var id = idProperty.GetValue(item);
                if (id != null)
                {
                    // Update existing or insert new
                    return collection.Upsert(item);
                }
            }
            
            // If no id property, just insert
            collection.Insert(item);
            return true;
        });
    }

    public Task<List<MeetupEvent>> GetUpcomingMeetupEvents()
    {
        const string cacheKey = "UpcomingMeetupEvents";

        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_databasePath);
                var collection = db.GetCollection<MeetupEvent>(Constants.MeetupEventsContainerId);
                
                var startDate = DateTime.Now;
                return collection.Find(x => DateTime.Parse(x.StartDateTime) >= startDate).ToList();
            });
        }, TimeSpan.FromMinutes(CacheTimespanShort)) ?? Task.FromResult(new List<MeetupEvent>());
    }
    
    public Task<List<MeetupGroup>> GetMeetupGroups()
    {
        const string cacheKey = "MeetupGroups";

        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_databasePath);
                var collection = db.GetCollection<MeetupGroup>(Constants.MeetupGroupsContainerId);
                
                return collection.FindAll().ToList();
            });
        }, TimeSpan.FromMinutes(CacheTimespanShort)) ?? Task.FromResult(new List<MeetupGroup>());
    }

    public Task<List<MeetupEvent>> GetAllMeetupEvents(DateTime startDate)
    {
        var cacheKey = $"AllMeetupEvents{startDate:yyyy-MM-dd}";
        
        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            var allGroups = await GetMeetupGroups();
            
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_databasePath);
                var collection = db.GetCollection<MeetupEvent>(Constants.MeetupEventsContainerId);
                
                var meetupEvents = collection.Find(x => DateTime.Parse(x.StartDateTime) >= startDate).ToList();

                foreach (var meetupEvent in meetupEvents)
                {
                    var group = allGroups.FirstOrDefault(x => x.Urlname.InvariantEquals(meetupEvent.Group.UrlName));
                    if (group != null)
                    {
                        meetupEvent.Group.Area = group.Area;
                    }
                    else
                    {
                        meetupEvent.Group.Area = "unknown";
                    }
                }
                
                return meetupEvents;
            });
        }, TimeSpan.FromMinutes(CacheTimespanLong)) ?? Task.FromResult(new List<MeetupEvent>());
    }
}