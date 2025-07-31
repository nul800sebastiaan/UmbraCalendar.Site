using System.Text.Json.Serialization;
using UmbraCalendar.Meetup.Models;

namespace UmbraCalendar.Meetup.Models.Groups;

public class Groups
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
} 

public class GroupAnalytics
{
    [JsonPropertyName("totalMembers")]
    public int? TotalMembers { get; set; }

    [JsonPropertyName("totalPastEvents")]
    public int? TotalPastEvents { get; set; }
}

public class Stats
{
    [JsonPropertyName("memberCounts")]
    public MemberCounts MemberCounts { get; set; }
}

public class MemberCounts
{
    [JsonPropertyName("all")]
    public int All { get; set; }
}