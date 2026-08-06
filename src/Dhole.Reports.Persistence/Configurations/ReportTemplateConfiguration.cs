using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Reports.Domain.Templates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Reports.Persistence.Configurations;

internal sealed class ReportTemplateConfiguration
    : EntityTypeConfigurationBase<ReportTemplate, Guid>
{
    public override void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        base.Configure(builder);
        builder.ToTable("report_templates");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("NOT is_deleted");
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasFilter("NOT is_deleted");
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.HtmlContent).HasColumnType("text").IsRequired();
        builder.Property(x => x.DesignerJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.DataSchemaJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SampleDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.PageSize).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Orientation).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PreviewPdf).HasColumnType("bytea").IsRequired();
        builder.Property(x => x.PreviewGeneratedAtUtc).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
