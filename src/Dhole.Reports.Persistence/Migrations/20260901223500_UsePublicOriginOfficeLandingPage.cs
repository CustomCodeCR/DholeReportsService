using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260901223500_UsePublicOriginOfficeLandingPage")]
public sealed class UsePublicOriginOfficeLandingPage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    'Escanee el código QR para ver o guardar la ficha de contacto de Castro Fallas en origen correspondiente a <strong>{{originOffice.polName}}</strong>. El código no abre Dhole ni concede acceso a sistemas internos.',
                    'Escanee el código QR para consultar la información pública de la oficina de Castro Fallas en origen correspondiente a <strong>{{originOffice.polName}}</strong>.'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    'Escanee el código QR para consultar la información pública de la oficina de Castro Fallas en origen correspondiente a <strong>{{originOffice.polName}}</strong>.',
                    'Escanee el código QR para ver o guardar la ficha de contacto de Castro Fallas en origen correspondiente a <strong>{{originOffice.polName}}</strong>. El código no abre Dhole ni concede acceso a sistemas internos.'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote';
            """);
    }
}
