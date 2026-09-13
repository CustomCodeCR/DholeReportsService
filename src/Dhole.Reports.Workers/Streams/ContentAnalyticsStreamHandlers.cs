using System.Text.Json;
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using Dhole.Reports.Domain.Analytics.Entities;
using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Reports.Workers.Streams;

internal abstract class ContentAnalyticsStreamHandlerBase(ServiceDbContext db, ILogger logger) : IRedisStreamMessageHandler
{
    public abstract string MessageType { get; }

    public async Task HandleAsync(RedisStreamEnvelope envelope, CancellationToken ct = default)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(envelope.PayloadJson) ? "{}" : envelope.PayloadJson);
        var root = document.RootElement;
        var id = ReadGuid(root, "analyticsEventId") ?? ReadGuid(root, "AnalyticsEventId") ?? ReadGuid(root, "meetingRequestId") ?? ReadGuid(root, "MeetingRequestId");
        if (!id.HasValue || id.Value == Guid.Empty)
        {
            logger.LogWarning("Ignoring analytics event without deterministic id. Type={MessageType}, MessageId={MessageId}", envelope.MessageType, envelope.MessageId);
            return;
        }
        if (await db.ContentAnalyticsEvents.AnyAsync(x => x.Id == id.Value, ct)) return;

        var item = ContentAnalyticsEvent.Create(
            id.Value,
            envelope.MessageType,
            ReadString(root, "siteKey") ?? ReadString(root, "SiteKey") ?? "main",
            ReadDate(root, "occurredAtUtc") ?? ReadDate(root, "OccurredAtUtc") ?? DateTime.UtcNow,
            contentId: ReadGuid(root, "contentId") ?? ReadGuid(root, "ContentId"),
            formId: ReadGuid(root, "formId") ?? ReadGuid(root, "FormId"),
            submissionId: ReadGuid(root, "submissionId") ?? ReadGuid(root, "SubmissionId"),
            meetingRequestId: ReadGuid(root, "meetingRequestId") ?? ReadGuid(root, "MeetingRequestId"),
            campaignId: ReadGuid(root, "campaignId") ?? ReadGuid(root, "CampaignId"),
            placementId: ReadGuid(root, "placementId") ?? ReadGuid(root, "PlacementId"),
            path: ReadString(root, "path") ?? ReadString(root, "Path"),
            locale: ReadString(root, "locale") ?? ReadString(root, "Locale"),
            interactionType: ReadString(root, "interactionType") ?? ReadString(root, "InteractionType"),
            targetKey: ReadString(root, "targetKey") ?? ReadString(root, "TargetKey"),
            sourceUrl: ReadString(root, "sourceUrl") ?? ReadString(root, "SourceUrl"),
            referrerUrl: ReadString(root, "referrerUrl") ?? ReadString(root, "ReferrerUrl"),
            utmSource: ReadString(root, "utmSource") ?? ReadString(root, "UtmSource"),
            utmMedium: ReadString(root, "utmMedium") ?? ReadString(root, "UtmMedium"),
            utmCampaign: ReadString(root, "utmCampaign") ?? ReadString(root, "UtmCampaign"),
            utmContent: ReadString(root, "utmContent") ?? ReadString(root, "UtmContent"),
            utmTerm: ReadString(root, "utmTerm") ?? ReadString(root, "UtmTerm"));
        db.ContentAnalyticsEvents.Add(item);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Recorded content analytics event {MessageType} ({AnalyticsEventId}).", envelope.MessageType, id.Value);
    }

    private static string? ReadString(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static Guid? ReadGuid(JsonElement root, string name) => root.TryGetProperty(name, out var value) && Guid.TryParse(value.ToString(), out var id) ? id : null;
    private static DateTime? ReadDate(JsonElement root, string name) => root.TryGetProperty(name, out var value) && DateTime.TryParse(value.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var date) ? date.ToUniversalTime() : null;
}

internal sealed class PageViewedStreamHandler(ServiceDbContext db, ILogger<PageViewedStreamHandler> logger) : ContentAnalyticsStreamHandlerBase(db, logger) { public override string MessageType => "content.analytics.page-viewed"; }
internal sealed class FormSubmittedStreamHandler(ServiceDbContext db, ILogger<FormSubmittedStreamHandler> logger) : ContentAnalyticsStreamHandlerBase(db, logger) { public override string MessageType => "content.analytics.form-submitted"; }
internal sealed class InteractionClickedStreamHandler(ServiceDbContext db, ILogger<InteractionClickedStreamHandler> logger) : ContentAnalyticsStreamHandlerBase(db, logger) { public override string MessageType => "content.analytics.interaction-clicked"; }
internal sealed class MeetingRequestedStreamHandler(ServiceDbContext db, ILogger<MeetingRequestedStreamHandler> logger) : ContentAnalyticsStreamHandlerBase(db, logger) { public override string MessageType => "content.meeting.requested"; }
