using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models.Groups;

public class Groups
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
} 

public class Data
{
    [JsonPropertyName("proNetwork")]
    public ProNetwork ProNetwork { get; set; }
    
    [JsonPropertyName("groupByUrlname")]
    public MeetupGroup MeetupGroup {  get; set; }
}

public class ProNetwork
{
    [JsonPropertyName("groupsSearch")]
    public GroupsSearch GroupsSearch { get; set; }
}

public class GroupsSearch
{
    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }

    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}

public class Edge
{
    [JsonPropertyName("node")]
    public MeetupGroup MeetupGroup { get; set; }
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
