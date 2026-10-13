using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261013221000_ShowFiscalStorageInPricingQuote")]
public sealed class ShowFiscalStorageInPricingQuote : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Both client quote templates are stored in PostgreSQL. Patch them in
        // place without replacing their existing branding, QR or custom details.
        // The matching resource HTML is updated for installations seeded later.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<div class="main-title">Cotización de transporte internacional</div>',
                    '<div class="main-title">{{rate.documentTitle}}</div>'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('<div class="main-title">Cotización de transporte internacional</div>' IN html_content) > 0;

            -- Wrap the whole transit section, not just its label. Customs-only
            -- and warehouse-only quotes must not display a false transit time.
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        html_content,
                        '<div class="transit">',
                        '{{#if rate.hasTransportService}}<div class="transit">'
                    ),
                    '<table class="summary-grid">',
                    '{{/if}}<table class="summary-grid">'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.hasTransportService}}' IN html_content) = 0
              AND POSITION('<div class="transit">' IN html_content) > 0
              AND POSITION('<table class="summary-grid">' IN html_content) > 0;

            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    '<tr><td class="k">Modalidad</td><td class="v">{{rate.shipmentMode}}</td></tr>',
                    '<tr><td class="k">Modalidad</td><td class="v">{{rate.shipmentMode}}</td></tr>{{#if rate.hasSelectedServices}}<tr><td class="k">Servicios solicitados</td><td class="v">{{rate.servicesSummary}}</td></tr>{{/if}}'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{rate.servicesSummary}}' IN html_content) = 0;

            -- Only show storage information when selected in Pricing or when
            -- a priced warehouse item is present in an older quotation.
            UPDATE reports.report_templates
            SET html_content = CASE
                    WHEN POSITION('<div class="section-title">Equipos cotizados</div>' IN html_content) > 0
                        THEN REPLACE(
                            html_content,
                            '<div class="section-title">Equipos cotizados</div>',
                            '{{#if rate.hasStorageService}}<div class="section-title">Servicio de almacenamiento</div><div class="terms-card neutral"><p><strong>Días de almacenaje fiscal:</strong> {{rate.storageDaysLabel}}</p><p>Los importes de almacenamiento incluidos son únicamente los conceptos detallados en esta cotización. Días adicionales y otros cargos se cobran solo si están cotizados expresamente.</p></div>{{/if}}<div class="section-title">Equipos cotizados</div>'
                        )
                    ELSE REPLACE(
                        html_content,
                        '<div class="section-title">Detalle de la cotización</div>',
                        '{{#if rate.hasStorageService}}<div class="section-title">Servicio de almacenamiento</div><div class="terms-card neutral"><p><strong>Días de almacenaje fiscal:</strong> {{rate.storageDaysLabel}}</p><p>Los importes de almacenamiento incluidos son únicamente los conceptos detallados en esta cotización. Días adicionales y otros cargos se cobran solo si están cotizados expresamente.</p></div>{{/if}}<div class="section-title">Detalle de la cotización</div>'
                    )
                END,
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.hasStorageService}}' IN html_content) = 0
              AND (
                  POSITION('<div class="section-title">Equipos cotizados</div>' IN html_content) > 0
                  OR POSITION('<div class="section-title">Detalle de la cotización</div>' IN html_content) > 0
              );

            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                html_content,
                                '<p>Los tiempos de tránsito son aproximados y están sujetos a conexiones, condiciones externas, revisiones y disponibilidad.</p>',
                                '{{#if rate.hasTransportService}}<p>Los tiempos de tránsito son aproximados y están sujetos a conexiones, condiciones externas, revisiones y disponibilidad.</p>{{/if}}'
                            ),
                            '<p>Espacios sujetos a disponibilidad al momento de reservar.</p>',
                            '{{#if rate.hasTransportService}}<p>Espacios sujetos a disponibilidad al momento de reservar.</p>{{/if}}'
                        ),
                        '<p>No incluye demoras, almacenajes, impuestos, permisos ni servicios no indicados expresamente.</p>',
                        '<p>Los servicios, cargos adicionales e impuestos no detallados expresamente en esta cotización no están incluidos.</p>'
                    ),
                    '<p>La carga debe estar correctamente embalada y rotulada para su transporte internacional.</p>',
                    '{{#if rate.hasTransportService}}<p>La carga debe estar correctamente embalada y rotulada para su transporte internacional.</p>{{/if}}'
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Presentation-only: do not revert client PDF fixes on rollback.
    }
}
