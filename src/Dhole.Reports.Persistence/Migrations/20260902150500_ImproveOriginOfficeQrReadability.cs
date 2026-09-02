using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260902150500_ImproveOriginOfficeQrReadability")]
public sealed class ImproveOriginOfficeQrReadability : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET html_content = replace(
                    replace(
                        html_content,
                        'width:132px; padding:12px; text-align:center; vertical-align:middle;',
                        'width:190px; padding:14px; text-align:center; vertical-align:middle;'
                    ),
                    'width:105px; height:105px; display:block; margin:0 auto;',
                    'width:165px; height:165px; display:block; margin:0 auto; object-fit:contain; image-rendering:pixelated;'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND html_content LIKE '%{{originOffice.qrDataUri}}%';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET html_content = replace(
                    replace(
                        html_content,
                        'width:190px; padding:14px; text-align:center; vertical-align:middle;',
                        'width:132px; padding:12px; text-align:center; vertical-align:middle;'
                    ),
                    'width:165px; height:165px; display:block; margin:0 auto; object-fit:contain; image-rendering:pixelated;',
                    'width:105px; height:105px; display:block; margin:0 auto;'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND html_content LIKE '%{{originOffice.qrDataUri}}%';
            """);
    }
}
