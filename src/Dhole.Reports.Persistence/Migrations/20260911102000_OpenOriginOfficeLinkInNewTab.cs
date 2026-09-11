using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260911102000_OpenOriginOfficeLinkInNewTab")]
public sealed class OpenOriginOfficeLinkInNewTab : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<div class="qr-url">{{originOffice.publicPageUrl}}</div>',
                    '<div class="qr-url"><a href="{{originOffice.publicPageUrl}}" target="_blank" rel="noopener noreferrer" style="color:inherit;text-decoration:underline;">{{originOffice.publicPageUrl}}</a></div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
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
                    '<div class="qr-url"><a href="{{originOffice.publicPageUrl}}" target="_blank" rel="noopener noreferrer" style="color:inherit;text-decoration:underline;">{{originOffice.publicPageUrl}}</a></div>',
                    '<div class="qr-url">{{originOffice.publicPageUrl}}</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }
}
