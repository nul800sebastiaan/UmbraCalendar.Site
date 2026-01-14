using UmbraCalendar.Database;
using UmbraCalendar.Meetup;
using Umbraco.Cms.Core.Composing;

namespace UmbraCalendar;

public class RegisterServices : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IDatabaseService, LiteDbService>();
        builder.Services.AddSingleton<Options>();
        builder.Services.AddSingleton<IMeetupService, MeetupService>();
        builder.Services.AddSingleton<IUpcomingMeetupService, UpcomingMeetupService>();

        var externalSettingsSection = builder.Config.GetSection("ExternalServices");
        builder.Services.Configure<Options>(externalSettingsSection);
    }
}