using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261015170000_ShowSelectedDestinationTaxInPricingQuote")]
public sealed class ShowSelectedDestinationTaxInPricingQuote : Migration
{
    private const string SqlDelimiter = "$dhole_quote_selected_tax$";
    private const string Anchor =
        "{{#if rate.hasSeparateCurrencyTotals}}<div class=\"totals-title\">Totales sin conversión</div>{{/if}}";
    private const string VatMarker = "{{#if rate.hasDestinationTax}}";
    private const string Breakdown =
        "\n      {{#if rate.hasDestinationTax}}\n"
        + "      <div style=\"margin:6px 0 10px; border-top:1px solid #ddd; padding-top:7px; font-size:9.5px;\">\n"
        + "        {{#each currencyTotals}}\n"
        + "        <div style=\"margin:3px 0; color:#444;\"><strong>{{currency}}</strong> · Subtotal: <strong>{{subtotal}}</strong> · IVA: <strong>{{tax}}</strong></div>\n"
        + "        {{/each}}\n"
        + "      </div>\n"
        + "      {{/if}}";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Quote templates are stored in PostgreSQL, not read from the embedded HTML
        // on each render. Patch existing FCL and LCL templates in place, once only.
        // Keep custom conditions, QR, branding and all previously applied changes.
        migrationBuilder.Sql(
            $"""
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    {SqlDelimiter}{Anchor}{SqlDelimiter},
                    {SqlDelimiter}{Anchor}{Breakdown}{SqlDelimiter}
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION({SqlDelimiter}{VatMarker}{SqlDelimiter} IN html_content) = 0
              AND POSITION({SqlDelimiter}{Anchor}{SqlDelimiter} IN html_content) > 0;
            """);

        // The original conditional is nested inside {{#each items}}. The binder
        // evaluates #if against the root data before expanding each item, so the
        // line-level IVA note never renders. An empty span is suppressed by CSS.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '<td class="concept">{{description}}{{#if destinationTaxLabel}}<span class="tax-note">{{destinationTaxLabel}}</span>{{/if}}{{#if notes}}<span class="notes">{{notes}}</span>{{/if}}</td>',
                        '<td class="concept">{{description}}<span class="tax-note">{{destinationTaxLabel}}</span>{{#if notes}}<span class="notes">{{notes}}</span>{{/if}}</td>'
                    ),
                    '.items-table .tax-note { display:block; color:#ad1220; font-size:8.2px; font-weight:700; margin-top:2px; }',
                    '.items-table .tax-note { display:block; color:#ad1220; font-size:8.2px; font-weight:700; margin-top:2px; }'
                        || E'\\n    .items-table .tax-note:empty { display:none; }'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND (
                  POSITION('{{#if destinationTaxLabel}}' IN html_content) > 0
                  OR POSITION('.items-table .tax-note:empty' IN html_content) = 0
              );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive change to persisted commercial templates.
    }
}
