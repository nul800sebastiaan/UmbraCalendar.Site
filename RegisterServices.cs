using Microsoft.AspNetCore.DataProtection;
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

        // Persist Data Protection keys to a stable location for Docker
        // This ensures OAuth tokens can be decrypted after container restarts
        var keysDirectory = Path.Combine(AppContext.BaseDirectory, "umbraco", "Data", "DataProtection-Keys");
        Directory.CreateDirectory(keysDirectory);
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

        var externalSettingsSection = builder.Config.GetSection("ExternalServices");
        builder.Services.Configure<Options>(externalSettingsSection);
    }
}