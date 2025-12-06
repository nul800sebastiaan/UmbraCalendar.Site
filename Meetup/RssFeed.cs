using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using UmbraCalendar.Database;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup;

public class RssController : RenderController
{
    private readonly IDatabaseService _databaseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    
    public RssController(ILogger<RenderController> logger, 
        ICompositeViewEngine compositeViewEngine, 
        IUmbracoContextAccessor umbracoContextAccessor,
        IDatabaseService databaseService,
        IServiceProvider serviceProvider,
        IUmbracoContextFactory umbracoContextFactory) 
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _databaseService = databaseService;
        _serviceProvider = serviceProvider;
        _umbracoContextFactory = umbracoContextFactory;
    }
      
    //[ResponseCache(Duration = 1200)]
    [HttpGet]
    public override IActionResult Index()
    {
        var blogCopyright = $"{DateTime.Now.Year} Umbracalendar";
        const string blogTitle = "Umbraco events on Meetup.com";
        const string blogDescription = "";
        const string blogUrl = "https://umbracalendar.com/";
        const string feedId = "MeetupRss";

        var feed = new SyndicationFeed(blogTitle, blogDescription, new Uri(blogUrl), feedId, DateTime.Now)
        {
            Copyright = new TextSyndicationContent(blogCopyright)
        };
        
        // Add RSS Event module namespace
        feed.AttributeExtensions.Add(new XmlQualifiedName("ev", "http://www.w3.org/2000/xmlns/"), "http://purl.org/rss/1.0/modules/event/");

        var items = new List<SyndicationItem>();

        using var _ = _umbracoContextFactory.EnsureUmbracoContext();
        using var serviceScope = _serviceProvider.CreateScope();
        var query = serviceScope.ServiceProvider.GetRequiredService<IPublishedContentQuery>();
        var rootNode = query.ContentAtRoot().FirstOrDefault();
        var eventsNode = rootNode?.Children().OfType<Events>().FirstOrDefault();
        
        if (eventsNode != null)
        {
            foreach (var umbracoEvent in eventsNode.Descendants().OfType<CalendarEvent>().Where(x => x.DateTo >= DateTime.Today))
            {
                if (umbracoEvent.EventLink?.Url == null) continue;
                
                var convertedDateFrom = DateTime.SpecifyKind(umbracoEvent.DateFrom, DateTimeKind.Local);
                var convertedDateTo = DateTime.SpecifyKind(umbracoEvent.DateTo, DateTimeKind.Local);
                var description = $"{convertedDateFrom:yyyy-MM-dd} from {convertedDateFrom:HH:mm} to {convertedDateTo:HH:mm} ({TimeZoneInfo.Local.DisplayName}) - {umbracoEvent.EventLocation}";
                
                if (umbracoEvent.DateFrom.ToString("yyyy-MM-dd") != umbracoEvent.DateTo.ToString("yyyy-MM-dd"))
                {
                    description = $"{umbracoEvent.DateFrom:yyyy-MM-dd} to {umbracoEvent.DateTo:yyyy-MM-dd} ({TimeZoneInfo.Local.DisplayName}) - {umbracoEvent.EventLocation}";
                }
                var item = new SyndicationItem(umbracoEvent.Name, description, new Uri(umbracoEvent.EventLink.Url))
                {
                    PublishDate = convertedDateFrom,
                    Id = umbracoEvent.Id.ToString(),
                    Summary = new TextSyndicationContent(description)
                };
                
                // Add RSS Event module fields
                item.ElementExtensions.Add("startdate", "http://purl.org/rss/1.0/modules/event/", convertedDateFrom.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                item.ElementExtensions.Add("enddate", "http://purl.org/rss/1.0/modules/event/", convertedDateTo.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                item.ElementExtensions.Add("location", "http://purl.org/rss/1.0/modules/event/", umbracoEvent.EventLocation ?? "");
                item.ElementExtensions.Add("organizer", "http://purl.org/rss/1.0/modules/event/", "Umbraco Community");
                item.ElementExtensions.Add("type", "http://purl.org/rss/1.0/modules/event/", "meetup");
                
                items.Add(item);
            }
        }
        
        var upcomingEvents = _databaseService.GetUpcomingMeetupEvents().Result;
        foreach (var meetupEvent in upcomingEvents)
        {
            var venueFormatted = string.Empty;
            if (meetupEvent.OnlineVenue != null)
            {
                venueFormatted = $" - Online";
            }
            else if(meetupEvent.Venue != null)
            {
                var array = new[] { meetupEvent.Venue.Name, meetupEvent.Venue.Address, meetupEvent.Venue.City, meetupEvent.Venue.Country.ToUpperInvariant() };
                var fullAddress = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
                venueFormatted = $" - {fullAddress}";
            }

            if (venueFormatted.Trim() == string.Empty)
            {
                venueFormatted = " - Needs a location";
            }
            var description = $"{meetupEvent.StartDateLocal} from {meetupEvent.StartTimeLocal} to {meetupEvent.EndTimeLocal} ({meetupEvent.Group?.Timezone}){venueFormatted}";
            var startDateTime = DateTimeOffset.Parse(meetupEvent.StartDateTime);
            var endDateTime = DateTimeOffset.Parse(meetupEvent.EndDateTime);
            var item = new SyndicationItem(meetupEvent.Title, description, new Uri(meetupEvent.EventUrl))
            {
                PublishDate = startDateTime,
                Id = meetupEvent.id,
                Summary = new TextSyndicationContent(description)
            };
            
            // Add RSS Event module fields
            item.ElementExtensions.Add("startdate", "http://purl.org/rss/1.0/modules/event/", startDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            item.ElementExtensions.Add("enddate", "http://purl.org/rss/1.0/modules/event/", endDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            
            var location = string.Empty;
            if (meetupEvent.OnlineVenue != null)
            {
                location = $"Online on {meetupEvent.OnlineVenue.Type}";
            }
            else if (meetupEvent.Venue != null)
            {
                var array = new[] { meetupEvent.Venue.Name, meetupEvent.Venue.Address, meetupEvent.Venue.City, meetupEvent.Venue.Country };
                location = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            
            item.ElementExtensions.Add("location", "http://purl.org/rss/1.0/modules/event/", location);
            item.ElementExtensions.Add("organizer", "http://purl.org/rss/1.0/modules/event/", meetupEvent.Group?.Name ?? "");
            item.ElementExtensions.Add("type", "http://purl.org/rss/1.0/modules/event/", meetupEvent.EventType?.ToLower() ?? "meetup");
            
            items.Add(item);
        }

        feed.Items = items.OrderBy(x => x.PublishDate);

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        using var stream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            var rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }
}