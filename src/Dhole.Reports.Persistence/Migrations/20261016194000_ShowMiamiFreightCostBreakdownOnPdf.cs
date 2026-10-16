using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261016194000_ShowMiamiFreightCostBreakdownOnPdf")]
public sealed class ShowMiamiFreightCostBreakdownOnPdf : Migration
{
    private const string Tag = "$dhole_miami_pdf_breakdown$";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The quote PDF uses the HTML persisted in PostgreSQL, not the embedded
        // template on disk. Update only the LCL template, leaving commercial
        // branding, QR, conditions, FCL quotes and user-edited HTML untouched.
        // The new span is empty for all other LCL sources, hence invisible.
        migrationBuilder.Sql(
            $"""
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '<td class="concept">{{{{description}}}}',
                        '<td class="concept">{{{{description}}}}<span class="miami-matrix-note">{{{{miamiBreakdown}}}}</span>'
                    ),
                    '</style>',
                    {Tag}
                    .items-table .miami-matrix-note {{ display:block; white-space:pre-line; line-height:1.45; color:#555; font-size:8px; font-weight:400; margin-top:4px; padding-left:7px; border-left:2px solid #ccc; }}
                    .items-table .miami-matrix-note:empty {{ display:none; }}
                    </style>
                    {Tag}
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-lcl-client-quote'
              AND NOT is_deleted
              AND POSITION('miami-matrix-note' IN html_content) = 0
              AND POSITION('<td class="concept">{{{{description}}}}' IN html_content) > 0;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Avoid destroying any subsequent customer-specific template changes.
    }
}
