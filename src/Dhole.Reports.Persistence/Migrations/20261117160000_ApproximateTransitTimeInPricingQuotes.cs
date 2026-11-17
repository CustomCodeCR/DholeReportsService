using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261117160000_ApproximateTransitTimeInPricingQuotes")]
public sealed class ApproximateTransitTimeInPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Production templates live in PostgreSQL; retain client-specific HTML and CSS.
        // REPLACE is idempotent and changes only the transit label.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                html_content,
                'Tiempo de tránsito</div>',
                'Tiempo aproximado de tránsito</div>'
            ), updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('Tiempo de tránsito</div>' IN html_content) > 0;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: do not reverse possible client edits to stored templates.
    }
}
