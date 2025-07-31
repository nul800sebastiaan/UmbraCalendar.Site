using System.Text.Json.Serialization;
using UmbraCalendar.Meetup.Models;

namespace UmbraCalendar.Meetup.Models.Groups;

public class MeetupGroup
{
    // Not annotated since CosmosDb really likes lower case `id`
    public string id { get; set; }

    [JsonPropertyName("keyGroupPhoto")]
    public Image KeyGroupPhoto { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("urlname")]
    public string Urlname { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("groupAnalytics")]
    public GroupAnalytics GroupAnalytics { get; set; }
    
    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }
    
    [JsonPropertyName("area")]
    public string Area { get; set; }
    
    [JsonPropertyName("upcomingEvents")]
    public EventsSearch UpcomingEvents { get; set; }

    [JsonPropertyName("pastEvents")]
    public EventsSearch PastEvents { get; set; }
    
    [JsonPropertyName("events")]
    public EventsSearch Events { get; set; }
    
    // Backward compatibility properties
    [JsonIgnore]
    public Image Logo => KeyGroupPhoto;
    
    [JsonIgnore]
    public Image GroupPhoto => KeyGroupPhoto;
}