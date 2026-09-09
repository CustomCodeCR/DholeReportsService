using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909225500_SetPricingClientQuoteBranding")]
public sealed class SetPricingClientQuoteBranding : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '<div class="main-title">Cotización de transporte internacional</div>',
                        '<div class="main-title">Cotización Logística Internacional</div>'
                    ),
                    '{{#if rate.showAgent}}<tr><td class="k">Agente / coloader</td><td class="v">{{rate.agent}}</td></tr>{{/if}}',
                    '<tr><td class="k">Agente / coloader</td><td class="v">Grupo Castro Fallas</td></tr>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '<div class="main-title">Cotización Logística Internacional</div>',
                        '<div class="main-title">Cotización de transporte internacional</div>'
                    ),
                    '<tr><td class="k">Agente / coloader</td><td class="v">Grupo Castro Fallas</td></tr>',
                    '{{#if rate.showAgent}}<tr><td class="k">Agente / coloader</td><td class="v">{{rate.agent}}</td></tr>{{/if}}'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }
}
