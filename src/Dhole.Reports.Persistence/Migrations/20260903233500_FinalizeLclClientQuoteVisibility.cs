using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260903233500_FinalizeLclClientQuoteVisibility")]
public sealed class FinalizeLclClientQuoteVisibility : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    '<span class="item-notes">{{notes}}</span>',
                    ''
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;

            UPDATE reports.report_templates
            SET html_content = regexp_replace(
                    html_content,
                    '<td>[[:space:]]*<span class="data-label">Días libres</span>[[:space:]]*<span class="data-value">\{\{rate\.freeDays\}\}</span>[[:space:]]*</td>',
                    '',
                    'gi'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-lcl-client-quote'
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
