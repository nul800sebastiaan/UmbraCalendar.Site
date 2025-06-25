using Microsoft.AspNetCore.Mvc;
using UmbraCalendar.Database;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace UmbraCalendar.Meetup.Controllers;

public class MeetupUpdateInfoController : SurfaceController
{
    private readonly IDatabaseService _databaseService;
    
    public MeetupUpdateInfoController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IDatabaseService databaseService) 
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAction(MeetupGroup meetupGroup)
    {
        var meetups = await _databaseService.GetMeetupGroups();
        var meetup = meetups.First(x => x.id == meetupGroup.id);

        if (!string.Equals(meetup.Area, meetupGroup.Area, StringComparison.InvariantCultureIgnoreCase))
        {
            meetup.Area = meetupGroup.Area;
            var result = await _databaseService.UpsertItemAsync(meetup, Constants.MeetupGroupsContainerId);
        }
        
        return RedirectToCurrentUmbracoPage();
    }
}