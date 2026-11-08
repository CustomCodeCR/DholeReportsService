using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Reports.Persistence.Messaging;
using Dhole.Reports.Domain.Analytics.Entities;
using Dhole.Reports.Domain.Templates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Reports.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options,
    IConfiguration? realtimeConfiguration = null) : AppDbContextBase(options)
{
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<ContentAnalyticsEvent> ContentAnalyticsEvents => Set<ContentAnalyticsEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddRealtimeTemplateInvalidations();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AddRealtimeTemplateInvalidations();
        return base.SaveChanges();
    }

    private void AddRealtimeTemplateInvalidations()
    {
        if (!ReportsRealtimeInvalidation.IsEnabled(realtimeConfiguration))
            return;
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries<ReportTemplate>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var templateId = entry.Entity.Id;
            if (templateId == Guid.Empty) continue;
            var eventId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var correlationId = eventId.ToString("D");
            OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventName = ReportsRealtimeInvalidation.Type,
                EventType = ReportsRealtimeInvalidation.Type,
                SourceService = "DholeReportsService",
                PayloadJson = ReportsRealtimeInvalidation.Serialize(
                    eventId, templateId, ReportsRealtimeInvalidation.NextVersionHint(),
                    now, correlationId),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = now.UtcDateTime
            });
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reports");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
