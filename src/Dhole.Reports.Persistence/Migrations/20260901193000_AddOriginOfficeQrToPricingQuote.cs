using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260901193000_AddOriginOfficeQrToPricingQuote")]
public sealed class AddOriginOfficeQrToPricingQuote : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    '<div class="validity">',
                    '<section class="section" style="page-break-inside:avoid; margin-top:14px;">'
                    || '<h2 class="section-title">Oficina en origen</h2>'
                    || '<table style="width:100%; border-collapse:collapse; border:1px solid #e5e7eb; border-radius:8px;">'
                    || '<tr><td style="width:132px; padding:12px; text-align:center; vertical-align:middle;">'
                    || '<img src="{{originOffice.qrDataUri}}" alt="QR oficina en origen" style="width:105px; height:105px; display:block; margin:0 auto;" />'
                    || '</td><td style="padding:12px; vertical-align:middle;">'
                    || '<div style="font-size:13px; font-weight:800; margin-bottom:6px;">{{originOffice.message}}</div>'
                    || '<div style="font-size:11px; line-height:1.5; color:#444;">Escanee el código QR para consultar contacto, teléfono, correo, dirección, coordenadas y fotografías de la oficina correspondiente a <strong>{{originOffice.polName}}</strong>.</div>'
                    || '</td></tr></table></section>'
                    || '<div class="validity">'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND html_content NOT LIKE '%{{originOffice.qrDataUri}}%';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // El bloque se conserva para no degradar cotizaciones emitidas con esta versión.
    }
}
