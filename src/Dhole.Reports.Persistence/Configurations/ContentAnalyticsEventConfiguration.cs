using Dhole.Reports.Domain.Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Reports.Persistence.Configurations;

internal sealed class ContentAnalyticsEventConfiguration : IEntityTypeConfiguration<ContentAnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<ContentAnalyticsEvent> builder)
    {
        builder.ToTable("content_analytics_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SiteKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(1024);
        builder.Property(x => x.Locale).HasMaxLength(16);
        builder.Property(x => x.InteractionType).HasMaxLength(32);
        builder.Property(x => x.TargetKey).HasMaxLength(200);
        builder.Property(x => x.SourceUrl).HasMaxLength(1024);
        builder.Property(x => x.ReferrerUrl).HasMaxLength(1024);
        builder.Property(x => x.UtmSource).HasMaxLength(200);
        builder.Property(x => x.UtmMedium).HasMaxLength(200);
        builder.Property(x => x.UtmCampaign).HasMaxLength(200);
        builder.Property(x => x.UtmContent).HasMaxLength(200);
        builder.Property(x => x.UtmTerm).HasMaxLength(200);
        builder.HasIndex(x => new { x.SiteKey, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.EventName, x.OccurredAtUtc });
        builder.HasIndex(x => x.ContentId);
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => x.FormId);
        builder.HasIndex(x => x.PlacementId);
    }
}
