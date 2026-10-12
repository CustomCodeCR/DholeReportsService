using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261012204500_ShowQuoteRevisionOnPdf")]
public sealed class ShowQuoteRevisionOnPdf : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // HTML templates are stored in reports.report_templates in PostgreSQL.
        // Updating only the embedded file would NOT update templates already in production.
        // Use the quote-number header if present; fall back to the first quote-number
        // placeholder in older FCL/LCL templates. Do not duplicate on retries.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = CASE
                    WHEN POSITION('<div class="quote-number">{{rate.quoteNumber}}</div>' IN html_content) > 0
                        THEN REPLACE(
                            html_content,
                            '<div class="quote-number">{{rate.quoteNumber}}</div>',
                            '<div class="quote-number">{{rate.quoteNumber}}</div><div class="quote-meta"><strong>REV {{rate.revisionNumber}}</strong></div>'
                        )
                    ELSE regexp_replace(
                        html_content,
                        '\{\{rate\.quoteNumber\}\}',
                        '{{rate.quoteNumber}} · REV {{rate.revisionNumber}}'
                    )
                END,
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{rate.revisionNumber}}' IN html_content) = 0;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Presentation-only: retain visible revision identifiers on rollback.
    }
}
