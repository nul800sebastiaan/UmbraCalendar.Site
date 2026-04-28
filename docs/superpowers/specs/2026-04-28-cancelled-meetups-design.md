# Cancelled meetups: badge, strikethrough, RSS metadata

Addresses [issue #2](https://github.com/nul800sebastiaan/UmbraCalendar.Site/issues/2): cancelled Meetup events still appear on the homepage with no indication they've been cancelled.

## Goal

When a Meetup event has `status == "CANCELLED"`:

1. It still appears in the upcoming-events list (not hidden).
2. The event title is rendered with strikethrough + reduced opacity.
3. A "Cancelled" badge is shown inline, immediately before the event title.
4. The RSS feed item carries a `<umbracalendar:cancelled>true</umbracalendar:cancelled>` element so downstream consumers can react.

Non-Meetup (manually authored) calendar events have no cancelled state today; they always render as not-cancelled and emit `false` in the RSS feed for shape consistency.

## Changes

### 1. `Meetup/Models/Events/MeetupEvent.cs`

- Add `[JsonPropertyName("status")] public string? Status { get; set; }`.
- Add `[JsonIgnore] public bool IsCancelled => string.Equals(Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);`.

`Status` is the raw value from the Meetup GraphQL API (e.g. `ACTIVE`, `CANCELLED`, `PAST`, `DRAFT`). `IsCancelled` is derived so we have one source of truth.

### 2. `Meetup/MeetupService.cs` — `GetEventDataQuery()`

Add `status` to the GraphQL field list. The same helper feeds three queries (pro-network upcoming search, per-group events, historic past search), so a single addition covers them all. Existing imports for events already in LiteDB will get `Status` populated on the next scheduled import via `UpsertItemAsync`.

### 3. `Calendar/Models/CalendarItem.cs`

Add `public bool IsCancelled { get; set; }`. Defaults to `false`; populated only from the Meetup mapping path.

### 4. `Meetup/UpcomingMeetupService.cs`

In the Meetup-to-`CalendarItem` mapping, set `IsCancelled = meetupEvent.IsCancelled`. The manual `CalendarEvent` mapping path leaves `IsCancelled` at its default `false` — Umbraco-authored events don't have a cancelled state.

No filtering: cancelled events stay in the returned list per the issue's resolution choice.

### 5. `Views/Home.cshtml`

- Add `is-cancelled` to the `.event` container's class list when `item.IsCancelled`.
- Inline before the title (`<h4 class="event-title">`), render `@if (item.IsCancelled) { <span class="cancelled-badge">Cancelled</span> }`.

The badge sits inside `.event-title` (option (a) — most prominent placement) so it reads as part of the title line, not as another flag.

### 6. `wwwroot/assets/css/main.css`

Add:

```css
.event.is-cancelled .event-title a {
    text-decoration: line-through;
    opacity: 0.6;
}

.cancelled-badge {
    display: inline-block;
    padding: 0.1em 0.5em;
    margin-right: 0.4em;
    border-radius: 3px;
    background: #c0392b;
    color: #fff;
    font-size: 0.75em;
    font-weight: 600;
    text-transform: uppercase;
    vertical-align: middle;
}
```

Colour and sizing are illustrative — match site conventions if a design token already exists.

### 7. `Meetup/RssFeed.cs`

For each Meetup syndication item, append (mirroring the existing `hqOrganizedEvent` pattern that already uses the `umbracalendar` namespace declared on line 59):

```csharp
item.ElementExtensions.Add(new XElement(
    XName.Get("cancelled", "https://umbracalendar.com/rss/"),
    meetupEvent.IsCancelled.ToString().ToLower()));
```

For Umbraco-authored events, emit the same element with value `false` so feed shape is uniform.

## Out of scope

- **Aggregation in `Events.cshtml`.** Per-area RSVP/attendee sums still include cancelled events. Worth a follow-up; the issue concerns the homepage display only.
- **Filtering cancelled out of the RSS feed.** Issue explicitly asks for metadata, not removal.
- **Astro client.** The screenshot in the issue is the Razor `Home.cshtml` view. The Astro client doesn't render `CalendarItem` and needs no change.
- **Backfill of existing cancelled events.** Once the GQL query includes `status`, the next scheduled import overwrites stored events with the new field. Manual backfill is unnecessary.

## Verification

- Build: `dotnet build` succeeds.
- Locally trigger the Meetup import job; verify a known-cancelled event in LiteDB now has `Status: "CANCELLED"`.
- Load `/`; confirm the cancelled event shows the badge and strikethrough.
- Load the RSS feed endpoint; confirm the cancelled item contains `<umbracalendar:cancelled>true</umbracalendar:cancelled>` and a non-cancelled item contains `false`.
