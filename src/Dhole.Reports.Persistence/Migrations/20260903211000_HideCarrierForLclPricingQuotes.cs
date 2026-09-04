using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260903211000_HideCarrierForLclPricingQuotes")]
public sealed class HideCarrierForLclPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = regexp_replace(
                    html_content,
                    '<td>[[:space:]]*<span class="data-label">Naviera</span>[[:space:]]*<span class="data-value">\{\{rate\.carrier\}\}</span>[[:space:]]*</td>',
                    '{{#if rate.showCarrier}}<td><span class="data-label">Naviera</span><span class="data-value">{{rate.carrier}}</span></td>{{/if}}',
                    'g'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND html_content NOT LIKE '%{{#if rate.showCarrier}}%';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = replace(
                    replace(html_content, '{{#if rate.showCarrier}}', ''),
                    '{{/if}}', ''
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote';
            """
        );
    }
}
