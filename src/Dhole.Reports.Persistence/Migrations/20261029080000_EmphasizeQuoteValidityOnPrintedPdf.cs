using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261029080000_EmphasizeQuoteValidityOnPrintedPdf")]
public sealed class EmphasizeQuoteValidityOnPrintedPdf : Migration
{
    private const string SqlTag = "$quote_validity_20261029$";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Active FCL/LCL HTML templates live in PostgreSQL. Update only the
        // client-adjacent validity row, the red strip and their print CSS.
        // This intentionally preserves branding, taxes, commercial amounts,
        // customs information, QR codes and custom user content.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<tr><td class="k">Cliente</td><td class="v">{{rate.clientName}}</td></tr>',
                    '<tr><td class="k">Cliente</td><td class="v">{{rate.clientName}}</td></tr>'
                    || CHR(10)
                    || '            <tr class="validity-detail"><td class="k">Vigencia</td><td class="v">{{rate.validFrom}} al {{rate.validTo}}</td></tr>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('<tr><td class="k">Cliente</td><td class="v">{{rate.clientName}}</td></tr>' IN html_content) > 0
              AND POSITION('class="validity-detail"' IN html_content) = 0;

            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<td><strong>Vigencia:</strong> {{rate.validFrom}} - {{rate.validTo}}</td>',
                    '<td><span class="validity-highlight">VIGENCIA: {{rate.validFrom}} AL {{rate.validTo}}</span></td>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('<td><strong>Vigencia:</strong> {{rate.validFrom}} - {{rate.validTo}}</td>' IN html_content) > 0
              AND POSITION('class="validity-highlight"' IN html_content) = 0;
            """
        );

        // Inject print-specific contrast rules only for the newly marked
        // versions. Guard every replacement to be safe on repeated deploys.
        var styleSql = """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '</style>',
                    __QUOTE_VALIDITY_TAG__
                    .accent-strip .validity-highlight { display:inline-block; color:#fff; font-size:11.5px; font-weight:900; letter-spacing:.25px; border-bottom:2px solid #fff; padding-bottom:1px; }
                    .kv .validity-detail .k { color:#ad1220; font-weight:800; }
                    .kv .validity-detail .v { color:#a20e1b; font-size:10.5px; font-weight:900; }
                    @media print {
                      .accent-strip td { background:#ad1220 !important; color:#fff !important; -webkit-print-color-adjust:exact; print-color-adjust:exact; }
                      .accent-strip .validity-highlight { color:#fff !important; border-bottom:2px solid #fff; font-weight:900; -webkit-print-color-adjust:exact; print-color-adjust:exact; }
                      .kv .validity-detail .k, .kv .validity-detail .v { font-weight:900; }
                    }
                    </style>
                    __QUOTE_VALIDITY_TAG__
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('<style>' IN html_content) > 0
              AND POSITION('class="validity-highlight"' IN html_content) > 0
              AND POSITION('.accent-strip .validity-highlight {' IN html_content) = 0;
            """;
        migrationBuilder.Sql(styleSql.Replace("__QUOTE_VALIDITY_TAG__", SqlTag, StringComparison.Ordinal));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Presentation-only and non-destructive: preserve subsequent edits.
    }
}
