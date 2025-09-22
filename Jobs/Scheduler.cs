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
            if (_runtimeState.Level < RuntimeLevel.Run) return;

            // These jobs only request from the database, does not use the Meetup API
            RecurringJob.AddOrUpdate<IMeetupService>($"🧠 Get Upcoming Umbraco Meetup events into cache", x =>
                x.GetUpcomingMeetupEvents(null), GetTiming("0 */2 * * *"));
            RecurringJob.AddOrUpdate<IMeetupService>($"🧠️ Get pro network Meetup groups into cache", x =>
                x.GetMeetupGroups(null), GetTiming("0 */2 * * *"));
            
            // Historic import - keep disabled
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import historic Umbraco Meetup events", x =>
                x.ImportHistoricMeetupEvents(null), Cron.Never);

            // These jobs use the Meetup API
            // Main import job - every 4 hours at :00
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import upcoming Umbraco Meetup events", x =>
                x.ImportUpcomingMeetupEvents(null), "0 */4 * * *");

            // Pro network groups - daily at 3:00 AM (avoids main import times)
            RecurringJob.AddOrUpdate<IMeetupService>($"️➡️ Import Umbraco pro network Meetup groups", x =>
                x.ImportNetworkGroups(null), "0 3 * * *");

            var outsideGroups = new List<string>
            {
                "umbraco-india-community",
                "cape-town-umbraco-meetup-group",
                "umbraco-nepal",
                "umbraco-germany",
                "dutch-umbraco-user-group"
            };
            
            // Calculate flexible scheduling
            var groupCount = outsideGroups.Count;
            var intervalMinutes = Math.Max(10, groupCount * 2); // At least 10 minutes, or 2 minutes per group
            const int gapMinutes = 2; // Minimum gap between tasks
            
            for (var i = 0; i < groupCount; i++)
            {
                var group = outsideGroups[i];

                // Group import: start at minute 1, then space by gapMinutes
                var groupMinute = 1 + (i * gapMinutes);

                // Event import: start after all group imports finish, then space by gapMinutes
                var eventMinute = 1 + (groupCount * gapMinutes) + 1 + (i * gapMinutes);

                RecurringJob.AddOrUpdate<IMeetupService>($"Ⓜ️ Get group {group} which is not in our pro network", x =>
                        x.ImportMeetupGroup(null, group), GetTiming($"{groupMinute} */{intervalMinutes} * * *"));

                RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import upcoming events for group {group}", x =>
                        x.ImportMeetupEventsForGroup(null, group, "upcoming"), GetTiming($"{eventMinute} */{intervalMinutes} * * *"));

                RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import past events for group {group}", x =>
                    x.ImportMeetupEventsForGroup(null, group, "past"), Cron.Never);
            }
        }

        private static string GetTiming(string timing)
        {
            return Environment.MachineName == "FLASH" || Environment.MachineName == "KILLERQUEEN" ? Cron.Never() : timing;
        }

        public void Terminate()
        {
        }
    }
}