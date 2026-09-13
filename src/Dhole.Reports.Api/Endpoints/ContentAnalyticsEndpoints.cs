using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Reports.Api.Endpoints;

public static class ContentAnalyticsEndpoints
{
    private const string PageViewed = "content.analytics.page-viewed";
    private const string FormSubmitted = "content.analytics.form-submitted";
    private const string InteractionClicked = "content.analytics.interaction-clicked";
    private const string MeetingRequested = "content.meeting.requested";

    public static IEndpointRouteBuilder MapContentAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports/content-analytics").WithTags("Content Analytics").RequireAuthorization();
        group.MapGet("/summary", SummaryAsync);
        group.MapGet("/pages", PagesAsync);
        group.MapGet("/campaigns", CampaignsAsync);
        group.MapGet("/forms", FormsAsync);
        group.MapGet("/interactions", InteractionsAsync);
        return app;
    }

    private static IQueryable<Dhole.Reports.Domain.Analytics.Entities.ContentAnalyticsEvent> Filter(ServiceDbContext db, string? siteKey, DateTime? fromUtc, DateTime? toUtc)
    {
        var query = db.ContentAnalyticsEvents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(siteKey)) query = query.Where(x => x.SiteKey == siteKey);
        if (fromUtc.HasValue) query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value.ToUniversalTime());
        if (toUtc.HasValue) query = query.Where(x => x.OccurredAtUtc <= toUtc.Value.ToUniversalTime());
        return query;
    }

    private static async Task<IResult> SummaryAsync(string? siteKey, DateTime? fromUtc, DateTime? toUtc, ServiceDbContext db, CancellationToken ct)
    {
        var query = Filter(db, siteKey, fromUtc, toUtc);
        var pageViews = await query.CountAsync(x => x.EventName == PageViewed, ct);
        var clicks = await query.CountAsync(x => x.EventName == InteractionClicked, ct);
        var forms = await query.CountAsync(x => x.EventName == FormSubmitted, ct);
        var meetings = await query.CountAsync(x => x.EventName == MeetingRequested, ct);
        return Results.Ok(new
        {
            pageViews,
            interactionClicks = clicks,
            formSubmissions = forms,
            meetingRequests = meetings,
            clickThroughRate = pageViews == 0 ? 0d : (double)clicks / pageViews,
            formConversionRate = pageViews == 0 ? 0d : (double)forms / pageViews,
            meetingConversionRate = forms == 0 ? 0d : (double)meetings / forms
        });
    }

    private static async Task<IResult> PagesAsync(string? siteKey, DateTime? fromUtc, DateTime? toUtc, int? take, ServiceDbContext db, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? 20, 1, 100);
        var rows = await Filter(db, siteKey, fromUtc, toUtc).Where(x => x.EventName == PageViewed && x.Path != null)
            .GroupBy(x => new { x.Path, x.ContentId }).Select(g => new { g.Key.Path, g.Key.ContentId, Visits = g.Count() })
            .OrderByDescending(x => x.Visits).Take(limit).ToListAsync(ct);
        return Results.Ok(rows);
    }

    private static async Task<IResult> CampaignsAsync(string? siteKey, DateTime? fromUtc, DateTime? toUtc, int? take, ServiceDbContext db, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? 20, 1, 100);
        var rows = await Filter(db, siteKey, fromUtc, toUtc).Where(x => x.CampaignId != null || x.UtmCampaign != null)
            .GroupBy(x => new { x.CampaignId, x.UtmCampaign })
            .Select(g => new
            {
                g.Key.CampaignId,
                g.Key.UtmCampaign,
                Visits = g.Count(x => x.EventName == PageViewed),
                Clicks = g.Count(x => x.EventName == InteractionClicked),
                FormSubmissions = g.Count(x => x.EventName == FormSubmitted),
                MeetingRequests = g.Count(x => x.EventName == MeetingRequested)
            })
            .OrderByDescending(x => x.FormSubmissions).ThenByDescending(x => x.Visits).Take(limit).ToListAsync(ct);
        return Results.Ok(rows);
    }

    private static async Task<IResult> FormsAsync(string? siteKey, DateTime? fromUtc, DateTime? toUtc, int? take, ServiceDbContext db, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? 20, 1, 100);
        var rows = await Filter(db, siteKey, fromUtc, toUtc).Where(x => x.EventName == FormSubmitted && x.FormId != null)
            .GroupBy(x => x.FormId).Select(g => new { FormId = g.Key, Conversions = g.Count() })
            .OrderByDescending(x => x.Conversions).Take(limit).ToListAsync(ct);
        return Results.Ok(rows);
    }

    private static async Task<IResult> InteractionsAsync(string? siteKey, DateTime? fromUtc, DateTime? toUtc, int? take, ServiceDbContext db, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? 20, 1, 100);
        var rows = await Filter(db, siteKey, fromUtc, toUtc).Where(x => x.EventName == InteractionClicked)
            .GroupBy(x => new { x.InteractionType, x.TargetKey, x.PlacementId })
            .Select(g => new { g.Key.InteractionType, g.Key.TargetKey, g.Key.PlacementId, Clicks = g.Count() })
            .OrderByDescending(x => x.Clicks).Take(limit).ToListAsync(ct);
        return Results.Ok(rows);
    }
}
