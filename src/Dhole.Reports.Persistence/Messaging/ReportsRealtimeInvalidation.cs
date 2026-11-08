using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Dhole.Reports.Persistence.Messaging;

/// <summary>
/// F6.09: report-template invalidation, never the HTML, design schema, sample
/// data, document bytes or other confidential report content.
/// </summary>
public static class ReportsRealtimeInvalidation
{
    public const string ReleaseId = "rt-20261107-01";
    public const string Type = "reports.template.changed.v1";
    public const string Stream = "dhole.reports.realtime.v1";
    private static long _lastVersionHint;

    public static bool IsEnabled(IConfiguration? config) =>
        config?.GetValue<bool>("Features:UniversalRealtimeEnabled") == true &&
        config.GetValue<bool>("Realtime:Outbox:Enabled") &&
        config.GetValue<bool>("Realtime:BatchApproved") &&
        string.Equals(config["Realtime:ReleaseId"], ReleaseId, StringComparison.Ordinal);

    public static long NextVersionHint()
    {
        while (true)
        {
            var now = (DateTimeOffset.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks) / 10;
            var before = Volatile.Read(ref _lastVersionHint);
            var next = Math.Max(now, before + 1);
            if (Interlocked.CompareExchange(ref _lastVersionHint, next, before) == before)
                return next;
        }
    }

    public static string Serialize(Guid eventId, Guid templateId, long versionHint,
        DateTimeOffset at, string correlationId)
    {
        if (eventId == Guid.Empty || templateId == Guid.Empty ||
            versionHint <= 0 || at.Offset != TimeSpan.Zero ||
            string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Invalid Reports realtime template.");
        return JsonSerializer.Serialize(new
        {
            eventId,
            eventType = Type,
            schemaVersion = 1,
            aggregateType = "template",
            aggregateId = templateId.ToString("D"),
            aggregateVersion = versionHint,
            occurredAtUtc = at,
            producer = "DholeReportsService",
            correlationId,
            audienceRef = $"reports:template:{templateId:D}",
            payload = new { changedFields = new[] { "template" } }
        });
    }
}
