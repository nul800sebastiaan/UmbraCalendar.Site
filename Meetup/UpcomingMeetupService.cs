﻿using HeyRed.MarkdownSharp;
using UmbraCalendar.Calendar.Models;
using UmbraCalendar.CosmosDb;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup;

public class UpcomingMeetupService : IUpcomingMeetupService
{
    private readonly ICosmosService _cosmosService;

    public UpcomingMeetupService(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<List<CalendarItem>> GetUpcomingMeetupEvents(List<CalendarEvent>? upcomingEvents)
    {
        var eventCalendar = new List<CalendarItem>();
        var meetups = await _cosmosService.GetUpcomingMeetupEvents();

        var mark = new Markdown();
        foreach (var meetupEvent in meetups)
        {
            var fullAddress = "";
            var country = "";
            if (meetupEvent.Venue != null)
            {
                country = meetupEvent.Venue.Country.ToUpperInvariant();
                var array = new[]
                {
                    meetupEvent.Venue.Name, meetupEvent.Venue.Address, meetupEvent.Venue.City,
                    meetupEvent.Venue.Country.ToUpperInvariant()
                };
                fullAddress = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
            }

            var item = new CalendarItem
            {
                Title = meetupEvent.Title,
                Description = mark.Transform(meetupEvent.Description),
                Banner = meetupEvent.Image?.Url,
                Location = fullAddress,
                DateTimeFrom = DateTime.Parse(meetupEvent.StartDateTime),
                DateTimeTo = DateTime.Parse(meetupEvent.EndDateTime),
                StartTimeLocal = meetupEvent.StartTimeLocal,
                EndTimeLocal = meetupEvent.EndTimeLocal,
                Url = meetupEvent.EventUrl,
                OnlineAvailable = meetupEvent.IsOnline,
                Tags = [],
                Source = "Meetup",
                Organizer = meetupEvent.Group.Name,
                Country = country,
                Going = meetupEvent.Going,
                EventType = meetupEvent.EventType
            };
            eventCalendar.Add(item);
        }

        if (upcomingEvents != null)
        {
            foreach (var calendarEvent in upcomingEvents)
            {
                var item = new CalendarItem
                {
                    Title = calendarEvent.Name,
                    Description = mark.Transform(calendarEvent.Description?.ToString()),
                    Banner = calendarEvent.BannerImage?.MediaUrl(),
                    EventHost = calendarEvent.EventHost,
                    EventIcon = calendarEvent.EventIcon?.MediaUrl(),
                    Location = calendarEvent.EventLocation,
                    DateTimeFrom = calendarEvent.DateFrom,
                    DateTimeTo = calendarEvent.DateTo,
                    Url = calendarEvent.EventLink?.Url,
                    OnlineAvailable = calendarEvent.OnlineAttendance,
                    Tags = (calendarEvent.Tags ?? Array.Empty<string>()).ToList(),
                    Source = "Manual",
                    Country = calendarEvent.Country
                };
                
                var eventType = "PHYSICAL"; // default type
                switch (calendarEvent.EventLocation)
                {
                    case "Online event":
                        eventType = "VIRTUAL";
                        break;
                    case "Hybrid event":
                        eventType = "HYBRID";
                        break;
                }

                item.EventType = eventType;

                eventCalendar.Add(item);
            }
        }

        return eventCalendar.OrderBy(x => x.DateTimeFrom).ToList();
    }
}