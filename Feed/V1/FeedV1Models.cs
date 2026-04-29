namespace UmbraCalendar.Feed.V1;

public class FeedDocument
{
    public FeedMeta Feed { get; set; } = default!;
    public IReadOnlyList<FeedEvent> Events { get; set; } = Array.Empty<FeedEvent>();
}

public class FeedMeta
{
    public string Title { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
}

public class FeedEvent
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string? Location { get; set; }
    public string? Organizer { get; set; }
    public string? ImageUrl { get; set; }
    public string AttendanceMode { get; set; } = "inPerson";
    public bool IsHqOrganized { get; set; }
    public bool IsCancelled { get; set; }
}
