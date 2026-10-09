using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261009170000_QuoteEquivalentCurrencyTotalsAndVolume")]
public sealed class QuoteEquivalentCurrencyTotalsAndVolume : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing reports.report_templates rows are persisted in the database:
        // changing the embedded HTML resource alone does not update production.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        REPLACE(
                            html_content,
                            '{{rate.totalVolumeCbm}} M3',
                            '{{rate.totalVolume}}'
                        ),
                        '{{#if rate.hasMultipleCurrencies}}<div class="totals-title">Totales por moneda</div>{{/if}}',
                        '{{#if rate.hasEquivalentCurrencies}}<div class="totals-title">Total general equivalente en USD y CRC</div>{{/if}}
                  {{#if rate.hasSeparateCurrencyTotals}}<div class="totals-title">Totales sin conversión</div>{{/if}}'
                    ),
                    '{{#if rate.hasMultipleCurrencies}}<div class="multi-note">Los importes de monedas distintas se presentan por separado y no se suman entre sí.</div>{{/if}}',
                    '{{#if rate.exchangeRateNote}}<div class="multi-note">{{rate.exchangeRateNote}}. Los valores USD y CRC representan el mismo total general.</div>{{/if}}
                  {{#if rate.hasSeparateCurrencyTotals}}<div class="multi-note">No se muestra equivalencia: falta un tipo de cambio válido o hay monedas distintas de USD/CRC.</div>{{/if}}'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Presentation-only migration: retain the validated modern template on rollback.
    }
}
