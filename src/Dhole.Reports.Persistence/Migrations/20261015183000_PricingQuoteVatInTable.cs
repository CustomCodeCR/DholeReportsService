using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261015183000_PricingQuoteVatInTable")]
public sealed class PricingQuoteVatInTable : Migration
{
    private const string HtmlResource = "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";
    private const string SqlDelimiter = "$dhole_iva_grid_20261015$";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The live FCL/LCL templates are persisted in PostgreSQL. Patch ONLY their
        // quote-detail table, totals and print styles; preserve branding, QR,
        // service descriptions and custom commercial terms in the stored HTML.
        using var stream = typeof(PricingQuoteVatInTable).Assembly.GetManifestResourceStream(HtmlResource)
            ?? throw new InvalidOperationException($"Embedded template missing: {HtmlResource}");
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        string Slice(string start, string end, bool includeEnd = false)
        {
            var from = template.IndexOf(start, StringComparison.Ordinal);
            if (from < 0) throw new InvalidOperationException($"PDF template marker missing: {start}");
            var to = template.IndexOf(end, from + start.Length, StringComparison.Ordinal);
            if (to < 0) throw new InvalidOperationException($"PDF template marker missing: {end}");
            return template.Substring(from, to - from + (includeEnd ? end.Length : 0));
        }

        var invoiceTable = Slice("<table class=\"items-table{{#if", "</table>", includeEnd: true);
        var summaryTable = Slice("<div class=\"totals\">", "</section>").TrimEnd();
        var styleRules = Slice("/* Compact invoice-style", ".header-table").TrimEnd();

        if (new[] { invoiceTable, summaryTable, styleRules }
            .Any(fragment => fragment.Contains(SqlDelimiter, StringComparison.Ordinal)))
            throw new InvalidOperationException("Reserved SQL delimiter present in PDF template.");

        migrationBuilder.Sql(
            $"""
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REGEXP_REPLACE(
                        REGEXP_REPLACE(
                            html_content,
                            '(?s)<table class="items-table">.*?</table>',
                            {SqlDelimiter}{invoiceTable}{SqlDelimiter},
                            'g'
                        ),
                        '(?s)<div class="totals">.*?</section>',
                        {SqlDelimiter}{summaryTable}
                        </section>{SqlDelimiter},
                        'g'
                    ),
                    '</style>',
                    {SqlDelimiter}{styleRules}
                    </style>{SqlDelimiter}
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('items-table--with-tax' IN html_content) = 0
              AND POSITION('<table class="items-table">' IN html_content) > 0
              AND POSITION('<div class="totals">' IN html_content) > 0;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive formatting migration: do not overwrite other template edits.
    }
}
