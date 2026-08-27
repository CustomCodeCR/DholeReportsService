using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827233000_EnsurePricingFclClientQuoteTemplate")]
public sealed class EnsurePricingFclClientQuoteTemplate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $body$
            BEGIN
                -- Pricing depende contractualmente de este código. Si alguien lo
                -- eliminó de forma lógica, se reactiva en vez de dejar fallar la impresión.
                IF NOT EXISTS (
                    SELECT 1
                    FROM reports.report_templates
                    WHERE code = 'pricing-fcl-client-quote'
                      AND NOT is_deleted
                ) THEN
                    UPDATE reports.report_templates
                    SET is_deleted = FALSE,
                        deleted_at_utc = NULL,
                        deleted_by = NULL,
                        is_active = TRUE,
                        updated_at_utc = NOW()
                    WHERE id = (
                        SELECT id
                        FROM reports.report_templates
                        WHERE code = 'pricing-fcl-client-quote'
                        ORDER BY created_at_utc DESC
                        LIMIT 1
                    );
                END IF;
            END
            $body$;

            INSERT INTO reports.report_templates (
                id,
                code,
                name,
                description,
                html_content,
                designer_json,
                data_schema_json,
                sample_data_json,
                page_size,
                orientation,
                preview_pdf,
                preview_generated_at_utc,
                is_active,
                created_at_utc,
                created_by,
                updated_at_utc,
                updated_by,
                is_deleted,
                deleted_at_utc,
                deleted_by)
            SELECT
                '8d584ff7-8425-427f-82c8-88c0c3a4bc01'::uuid,
                'pricing-fcl-client-quote',
                'Cotización Pricing - Cliente',
                'Plantilla obligatoria para imprimir cotizaciones de Pricing.',
                $html$
                <!doctype html>
                <html lang="es">
                <head>
                  <meta charset="utf-8" />
                  <style>
                    @page { size: A4; margin: 15mm; }
                    * { box-sizing: border-box; }
                    body { margin: 0; font-family: Arial, Helvetica, sans-serif; color: #172033; font-size: 11px; line-height: 1.45; }
                    .header { display:flex; justify-content:space-between; gap:20px; border-bottom:2px solid #172033; padding-bottom:12px; margin-bottom:16px; }
                    .brand { font-size:20px; font-weight:800; }
                    .muted { color:#667085; }
                    .quote { text-align:right; }
                    .quote strong { display:block; font-size:16px; }
                    .grid { display:grid; grid-template-columns:1fr 1fr; gap:8px 18px; margin-bottom:16px; }
                    .field { border-bottom:1px solid #e4e7ec; padding:5px 0; }
                    .field b { display:block; font-size:9px; text-transform:uppercase; color:#667085; }
                    table { width:100%; border-collapse:collapse; margin-top:10px; }
                    th { background:#f2f4f7; text-align:left; font-size:9px; text-transform:uppercase; padding:8px 7px; border-bottom:1px solid #d0d5dd; }
                    td { padding:8px 7px; border-bottom:1px solid #eaecf0; vertical-align:top; }
                    .num { text-align:right; white-space:nowrap; }
                    .total { margin:14px 0 18px auto; width:260px; padding:10px 12px; border:1px solid #d0d5dd; border-radius:8px; text-align:right; }
                    .total span { display:block; color:#667085; font-size:9px; text-transform:uppercase; }
                    .total strong { font-size:17px; }
                    .terms { margin-top:12px; page-break-inside:avoid; }
                    .terms h3 { margin:0 0 5px; font-size:11px; }
                    .preline { white-space:pre-line; color:#475467; }
                    .footer { margin-top:22px; padding-top:10px; border-top:1px solid #d0d5dd; color:#667085; font-size:9px; }
                  </style>
                </head>
                <body>
                  <div class="header">
                    <div><div class="brand">{{company.name}}</div><div class="muted">{{company.website}}</div></div>
                    <div class="quote"><span class="muted">Cotización</span><strong>{{rate.quoteNumber}}</strong><span>{{generated.date}}</span></div>
                  </div>
                  <div class="grid">
                    <div class="field"><b>Cliente</b>{{rate.clientName}}</div>
                    <div class="field"><b>Ruta</b>{{rate.route}}</div>
                    <div class="field"><b>Naviera / proveedor</b>{{rate.carrier}}</div>
                    <div class="field"><b>Equipo / embarque</b>{{rate.containerSummary}}</div>
                    <div class="field"><b>Tránsito</b>{{rate.transitTime}}</div>
                    <div class="field"><b>Días libres</b>{{rate.freeDays}}</div>
                    <div class="field"><b>Vigencia desde</b>{{rate.validFrom}}</div>
                    <div class="field"><b>Vigencia hasta</b>{{rate.validTo}}</div>
                  </div>
                  <table>
                    <thead><tr><th>Concepto</th><th class="num">Cantidad</th><th class="num">Precio unitario</th><th class="num">Total</th></tr></thead>
                    <tbody>
                      {{#each items}}
                      <tr><td>{{description}}<div class="muted">{{notes}}</div></td><td class="num">{{quantity}}</td><td class="num">{{unitSale}}</td><td class="num">{{lineTotal}}</td></tr>
                      {{/each}}
                    </tbody>
                  </table>
                  <div class="total"><span>Total cotización</span><strong>{{rate.total}}</strong></div>
                  <div class="terms"><h3>Tarifa incluye</h3><div class="preline">{{rate.includes}}</div></div>
                  <div class="terms"><h3>Sujeto a</h3><div class="preline">{{rate.subjectTo}}</div></div>
                  <div class="terms"><h3>Tarifa no incluye</h3><div class="preline">{{rate.excludes}}</div></div>
                  <div class="footer">Tiempo de tránsito estimado y sujeto a cambios. Espacios sujetos a disponibilidad. No incluye demoras ni servicios no indicados expresamente en esta cotización.</div>
                </body>
                </html>
                $html$,
                '{}'::jsonb,
                '{}'::jsonb,
                '{}'::jsonb,
                'A4',
                'Portrait',
                decode('', 'hex'),
                NOW(),
                TRUE,
                NOW(),
                NULL,
                NULL,
                NULL,
                FALSE,
                NULL,
                NULL
            WHERE NOT EXISTS (
                SELECT 1
                FROM reports.report_templates
                WHERE code = 'pricing-fcl-client-quote'
                  AND NOT is_deleted
            );

            UPDATE reports.report_templates
            SET is_active = TRUE
            WHERE code = 'pricing-fcl-client-quote'
              AND NOT is_deleted;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM reports.report_templates
            WHERE id = '8d584ff7-8425-427f-82c8-88c0c3a4bc01'::uuid
              AND code = 'pricing-fcl-client-quote';
            """);
    }
}
