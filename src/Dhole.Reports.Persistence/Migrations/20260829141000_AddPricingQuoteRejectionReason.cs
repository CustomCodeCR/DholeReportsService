using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260829141000_AddPricingQuoteRejectionReason")]
public sealed class AddPricingQuoteRejectionReason : Migration
{
    private const string RejectionSection = """
{{#if rate.rejectionReason}}
<section class="section" style="page-break-inside: avoid;">
  <h2 class="section-title">Motivo de rechazo</h2>
  <div style="padding: 12px 14px; border: 1px solid #fecaca; border-left: 5px solid #dc2626; border-radius: 6px; background: #fef2f2; color: #7f1d1d; font-size: 10px; line-height: 1.55; white-space: pre-line;">
    {{rate.rejectionReason}}
  </div>
</section>
{{/if}}

""";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($$"""
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    '<div class="validity">',
                    $insert${{RejectionSection}}<div class="validity">$insert$
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND html_content NOT LIKE '%rate.rejectionReason%';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($$"""
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    $insert${{RejectionSection}}<div class="validity">$insert$,
                    '<div class="validity">'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote';
            """);
    }
}
