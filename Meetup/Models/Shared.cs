using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models;

public class Data
{
    [JsonPropertyName("proNetwork")]
    public ProNetwork ProNetwork { get; set; }
    
    [JsonPropertyName("groupByUrlname")]
    public Groups.MeetupGroup GroupByUrlname { get; set; }
    
    // Backward compatibility properties
    [JsonIgnore]
    public ProNetwork MeetupNetwork => ProNetwork;
    
    [JsonIgnore]
    public Groups.MeetupGroup MeetupGroup => GroupByUrlname;
}

public class ProNetwork
{
    [JsonPropertyName("eventsSearch")]
    public EventsSearch EventsSearch { get; set; }
    
    [JsonPropertyName("groupsSearch")]
    public GroupsSearch GroupsSearch { get; set; }
}


public class EventsSearch
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }
    [JsonPropertyName("edges")]
    public EventEdge[] Edges { get; set; }
}

public class GroupsSearch
{
    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }

    [JsonPropertyName("edges")]
    public List<GroupEdge> Edges { get; set; }
}

public class EventEdge
{
    [JsonPropertyName("node")]
    public Events.MeetupEvent MeetupEvent { get; set; }
}

public class GroupEdge
{
    [JsonPropertyName("node")]
    public Groups.MeetupGroup MeetupGroup { get; set; }
}

public class RsvpEdge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}

public class Node
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("member")]
    public Member Member { get; set; }
}

public class Member
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("zip")]
    public object Zip { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }

    [JsonPropertyName("memberPhoto")]
    public MemberPhoto MemberPhoto { get; set; }
}

public class MemberPhoto
{
    [JsonPropertyName("baseUrl")]
    public string Source { get; set; }
}