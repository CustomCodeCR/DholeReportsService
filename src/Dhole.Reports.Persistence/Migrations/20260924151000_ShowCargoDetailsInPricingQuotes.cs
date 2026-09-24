using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260924151000_ShowCargoDetailsInPricingQuotes")]
public sealed class ShowCargoDetailsInPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<div class="section-title">Equipos cotizados</div>',
                    '{{#if rate.cargoDetails}}<div class="section-title">Detalles de la carga</div><div class="terms-card neutral"><p>{{rate.cargoDetails}}</p></div>{{/if}}<div class="section-title">Equipos cotizados</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.cargoDetails}}' IN html_content) = 0;
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
                    '{{#if rate.cargoDetails}}<div class="section-title">Detalles de la carga</div><div class="terms-card neutral"><p>{{rate.cargoDetails}}</p></div>{{/if}}<div class="section-title">Equipos cotizados</div>',
                    '<div class="section-title">Equipos cotizados</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }
}
