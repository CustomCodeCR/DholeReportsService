using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260902143000_AddPricingOriginQrCode")]
public sealed class AddPricingOriginQrCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET
                html_content = REPLACE(
                    html_content,
                    '<footer class="footer">',
                    $origin$
                    {{#if rate.originUrl}}
                    <section style="margin-top: 18px; padding: 13px 14px; border: 1px solid #e6e6e6; border-radius: 7px; background: #fafafa; page-break-inside: avoid;">
                      <table style="width: 100%; border-collapse: collapse;">
                        <tr>
                          <td style="padding: 0 14px 0 0; border: 0; vertical-align: middle;">
                            <div style="margin-bottom: 4px; font-size: 10px; font-weight: 800; letter-spacing: .6px; color: #fc2800; text-transform: uppercase;">
                              Oficina / WHS en origen
                            </div>
                            <div style="font-size: 12px; font-weight: 800; color: #030202;">
                              {{rate.originQrText}}
                            </div>
                            <div style="margin-top: 5px; font-size: 9px; line-height: 1.45; color: #666666;">
                              Escanee el código QR para consultar la oficina correspondiente al POL y a esta ruta, incluyendo contacto, teléfono, correo, dirección, coordenadas y fotografías disponibles.
                            </div>
                          </td>
                          <td style="width: 112px; padding: 0; border: 0; text-align: right; vertical-align: middle;">
                            <img
                              src="{{qr rate.originUrl}}"
                              alt="QR datos de Castro Fallas en origen"
                              style="display: inline-block; width: 104px; height: 104px; padding: 4px; background: #ffffff; border: 1px solid #dddddd;"
                            />
                          </td>
                        </tr>
                      </table>
                    </section>
                    {{/if}}

                    <footer class="footer">
                    $origin$
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-fcl-client-quote'
              AND is_deleted = FALSE
              AND html_content NOT LIKE '%{{qr rate.originUrl}}%';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally left empty. The report template is editable by users after deployment,
        // so a rollback must not overwrite later manual template changes.
    }
}
