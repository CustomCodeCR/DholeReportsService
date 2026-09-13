namespace Dhole.Reports.Domain.Analytics.Entities;

public sealed class ContentAnalyticsEvent
{
    private ContentAnalyticsEvent() { }

    private ContentAnalyticsEvent(Guid id, string eventName, string siteKey, DateTime occurredAtUtc)
    {
        Id = id;
        EventName = eventName;
        SiteKey = siteKey;
        OccurredAtUtc = occurredAtUtc.Kind == DateTimeKind.Utc ? occurredAtUtc : occurredAtUtc.ToUniversalTime();
        ReceivedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string EventName { get; private set; } = string.Empty;
    public string SiteKey { get; private set; } = "main";
    public Guid? ContentId { get; private set; }
    public Guid? FormId { get; private set; }
    public Guid? SubmissionId { get; private set; }
    public Guid? MeetingRequestId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public Guid? PlacementId { get; private set; }
    public string? Path { get; private set; }
    public string? Locale { get; private set; }
    public string? InteractionType { get; private set; }
    public string? TargetKey { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? ReferrerUrl { get; private set; }
    public string? UtmSource { get; private set; }
    public string? UtmMedium { get; private set; }
    public string? UtmCampaign { get; private set; }
    public string? UtmContent { get; private set; }
    public string? UtmTerm { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }

    public static ContentAnalyticsEvent Create(
        Guid id,
        string eventName,
        string siteKey,
        DateTime occurredAtUtc,
        Guid? contentId = null,
        Guid? formId = null,
        Guid? submissionId = null,
        Guid? meetingRequestId = null,
        Guid? campaignId = null,
        Guid? placementId = null,
        string? path = null,
        string? locale = null,
        string? interactionType = null,
        string? targetKey = null,
        string? sourceUrl = null,
        string? referrerUrl = null,
        string? utmSource = null,
        string? utmMedium = null,
        string? utmCampaign = null,
        string? utmContent = null,
        string? utmTerm = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Analytics event id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("Event name is required.", nameof(eventName));
        var item = new ContentAnalyticsEvent(id, Limit(eventName, 120)!, Limit(siteKey, 64) ?? "main", occurredAtUtc)
        {
            ContentId = contentId,
            FormId = formId,
            SubmissionId = submissionId,
            MeetingRequestId = meetingRequestId,
            CampaignId = campaignId,
            PlacementId = placementId,
            Path = Limit(path, 1024),
            Locale = Limit(locale, 16),
            InteractionType = Limit(interactionType, 32),
            TargetKey = Limit(targetKey, 200),
            SourceUrl = Limit(sourceUrl, 1024),
            ReferrerUrl = Limit(referrerUrl, 1024),
            UtmSource = Limit(utmSource, 200),
            UtmMedium = Limit(utmMedium, 200),
            UtmCampaign = Limit(utmCampaign, 200),
            UtmContent = Limit(utmContent, 200),
            UtmTerm = Limit(utmTerm, 200)
        };
        return item;
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
