using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260917190500_ForceCastroFallasAgentInPricingQuotes")]
public sealed class ForceCastroFallasAgentInPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    $agent_old${{#if rate.showAgent}}<tr><td class="k">Agente / coloader</td><td class="v">{{rate.agent}}</td></tr>{{/if}}$agent_old$,
                    $agent_new$<tr><td class="k">Agente / coloader</td><td class="v">Grupo Castro Fallas</td></tr>$agent_new$
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
                    html_content,
                    $agent_new$<tr><td class="k">Agente / coloader</td><td class="v">Grupo Castro Fallas</td></tr>$agent_new$,
                    $agent_old${{#if rate.showAgent}}<tr><td class="k">Agente / coloader</td><td class="v">{{rate.agent}}</td></tr>{{/if}}$agent_old$
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }
}
