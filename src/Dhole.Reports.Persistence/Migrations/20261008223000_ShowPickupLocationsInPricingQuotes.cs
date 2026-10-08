using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261008223000_ShowPickupLocationsInPricingQuotes")]
public sealed class ShowPickupLocationsInPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<div class="section-title">Equipos cotizados</div>',
                    '{{#if rate.pickupLocations}}<div class="section-title">Puntos de recolecta</div>{{#each rate.pickupLocations}}<div class="terms-card neutral"><strong>{{label}}</strong> {{address}}{{classification}}</div>{{/each}}{{/if}}<div class="section-title">Equipos cotizados</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.pickupLocations}}' IN html_content) = 0
              AND POSITION('<div class="section-title">Equipos cotizados</div>' IN html_content) > 0;
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
                    '{{#if rate.pickupLocations}}<div class="section-title">Puntos de recolecta</div>{{#each rate.pickupLocations}}<div class="terms-card neutral"><strong>{{label}}</strong> {{address}}{{classification}}</div>{{/each}}{{/if}}<div class="section-title">Equipos cotizados</div>',
                    '<div class="section-title">Equipos cotizados</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.pickupLocations}}' IN html_content) > 0;
            """
        );
    }
}
