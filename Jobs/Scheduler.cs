using Hangfire;
using UmbraCalendar.Meetup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;

namespace UmbraCalendar.Jobs;

public class Scheduler : IComposer
{
    
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Components().Append<SchedulerComponent>();
    }

    public class SchedulerComponent : IComponent
    {
        private readonly IRuntimeState _runtimeState;

        public SchedulerComponent(IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            
        }

        public void Initialize()
        {
            if(_runtimeState.Level < RuntimeLevel.Run) return;
            
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import upcoming Umbraco Meetup events", x =>
                // TODO: remove hack - this runs every 30 minutes so that the meetup OAuth token is refreshed before it expires
                x.ImportUpcomingMeetupEvents(null), "0,30 * * * *");
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import historic Umbraco Meetup events", x =>
                x.ImportHistoricMeetupEvents(null), Cron.Never);
            RecurringJob.AddOrUpdate<IMeetupService>($"️➡️ Import Umbraco pro network Meetup groups", x =>
                x.ImportNetworkGroups(null), Cron.Daily());
            
            RecurringJob.AddOrUpdate<IMeetupService>($"🧠 Get Upcoming Umbraco Meetup events into cache", x =>
                x.GetUpcomingMeetupEvents(null), GetTiming("0 */2 * * *"));
            RecurringJob.AddOrUpdate<IMeetupService>($"🧠️ Get pro network Meetup groups into cache", x =>
                x.GetMeetupGroups(null), GetTiming("0 */2 * * *"));
            
            var outsideGroups = new List<string>
            {
                "umbraco-india-community",
                "cape-town-umbraco-meetup-group",
                "umbraco-nepal", 
                "umbraco-germany",
                "dutch-umbraco-user-group"
            };
            foreach (var outsideGroup in outsideGroups)
            {
                RecurringJob.AddOrUpdate<IMeetupService>($"Ⓜ️ Get group {outsideGroup} which is not in our pro network", x =>
                    x.ImportMeetupGroup(null, outsideGroup), GetTiming("0 */2 * * *"));
                
                RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import upcoming events for group {outsideGroup}", x =>
                    x.ImportMeetupEventsForGroup(null, outsideGroup, "upcoming"), GetTiming("30 */2 * * *"));
                
                RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import past events for group {outsideGroup}", x =>
                    x.ImportMeetupEventsForGroup(null, outsideGroup, "past"), Cron.Never);
            }
        }
        
        private static string GetTiming(string timing)
        {
            return Environment.MachineName == "FLASH" || Environment.MachineName == "KILLERQUEEN" ? Cron.Never() : timing;
        }

        public void Terminate()
        { }
    }
}