using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261013024000_HideExchangeRateInPricingQuotes")]
public sealed class HideExchangeRateInPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Quote templates are persisted in PostgreSQL: update both existing FCL/LCL
        // HTML templates as well as the embedded resource used for new templates.
        // Keep all conversion calculations and USD/CRC totals unchanged.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '{{#if rate.exchangeRateNote}}<div class="multi-note">{{rate.exchangeRateNote}}. Los valores USD y CRC representan el mismo total general.</div>{{/if}}',
                        '{{#if rate.hasEquivalentCurrencies}}<div class="multi-note">Los valores USD y CRC representan el mismo total general.</div>{{/if}}'
                    ),
                    '{{#if rate.hasSeparateCurrencyTotals}}<div class="multi-note">No se muestra equivalencia: falta un tipo de cambio válido o hay monedas distintas de USD/CRC.</div>{{/if}}',
                    '{{#if rate.hasSeparateCurrencyTotals}}<div class="multi-note">No se presenta equivalencia entre monedas; los totales se muestran sin conversión.</div>{{/if}}'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND (
                  POSITION('{{rate.exchangeRateNote}}' IN html_content) > 0
                  OR POSITION('falta un tipo de cambio válido' IN html_content) > 0
              );
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Presentation-only change: do not restore exchange-rate disclosure in client PDFs.
    }
}
