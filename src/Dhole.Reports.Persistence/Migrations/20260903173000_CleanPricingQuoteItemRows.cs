using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260903173000_CleanPricingQuoteItemRows")]
public sealed class CleanPricingQuoteItemRows : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- El PDF comercial debe mostrar únicamente el concepto de cada línea.
            -- Notes contiene trazabilidad interna del Excel/cálculo y no es texto
            -- que deba exponerse al cliente.
            UPDATE reports.report_templates
            SET html_content = replace(
                    html_content,
                    '<span class="item-notes">{{notes}}</span>',
                    ''
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;

            -- LCL no utiliza días libres de contenedor. Se elimina el bloque en la
            -- plantilla dedicada sin alterar la plantilla FCL.
            UPDATE reports.report_templates
            SET html_content = regexp_replace(
                    html_content,
                    '<td>[[:space:]]*<span class="data-label">Días libres</span>[[:space:]]*<span class="data-value">\{\{rate\.freeDays\}\}</span>[[:space:]]*</td>',
                    '',
                    'gi'
                ),
                updated_at_utc = NOW()
            WHERE code = 'pricing-lcl-client-quote'
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No se restauran comentarios internos ni días libres en LCL porque su
        // exposición al cliente era el comportamiento incorrecto.
    }
}
