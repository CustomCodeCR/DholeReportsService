using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261106203000_AddCarrierDetailsToPricingQuotePdf")]
public sealed class AddCarrierDetailsToPricingQuotePdf : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Templates in production are database records. Preserve client-specific
        // branding and only augment the transit section for both FCL and LCL.
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                html_content,
                '<div class="transit-value">{{rate.transitTime}}</div>',
                $carrier_fields$
                <div class="transit-value">{{rate.transitTime}}</div>
                {{#if rate.carrierRoute}}
                <div class="transit-extra">
                  <div class="transit-label">Ruta de la naviera</div>
                  <div class="transit-note">{{rate.carrierRoute}}</div>
                </div>
                {{/if}}
                {{#if rate.carrierComments}}
                <div class="transit-extra">
                  <div class="transit-label">Comentarios de la naviera</div>
                  <div class="transit-note">{{rate.carrierComments}}</div>
                </div>
                {{/if}}
                $carrier_fields$
            ), updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('<div class="transit-value">{{rate.transitTime}}</div>' IN html_content) > 0
              AND POSITION('{{rate.carrierRoute}}' IN html_content) = 0;

            UPDATE reports.report_templates
            SET html_content = REPLACE(
                html_content,
                '</style>',
                $carrier_css$
                .transit { display:block !important; }
                .transit-extra { margin-top:6px; }
                .transit-note { font-size:10px; font-weight:600; white-space:pre-wrap; overflow-wrap:anywhere; }
                </style>
                $carrier_css$
            ), updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{rate.carrierRoute}}' IN html_content) > 0
              AND POSITION('.transit-extra {' IN html_content) = 0;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: maintain later user edits of stored templates.
    }
}
