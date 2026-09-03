using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260903224500_AddPricingLclClientQuoteTemplate")]
public sealed class AddPricingLclClientQuoteTemplate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $body$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM reports.report_templates
                    WHERE code = 'pricing-lcl-client-quote'
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
                        WHERE code = 'pricing-lcl-client-quote'
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
                'd153b5d1-cc7f-4ec0-9b8e-628e89e96117'::uuid,
                'pricing-lcl-client-quote',
                'Cotización Pricing LCL - Cliente',
                'Plantilla exclusiva para cotizaciones LCL. Presenta el embarque por CBM y no expone la naviera utilizada por el consolidado.',
                html_content,
                designer_json,
                data_schema_json,
                sample_data_json,
                page_size,
                orientation,
                decode('', 'hex'),
                NOW(),
                TRUE,
                NOW(),
                NULL,
                NOW(),
                NULL,
                FALSE,
                NULL,
                NULL
            FROM reports.report_templates
            WHERE code = 'pricing-fcl-client-quote'
              AND NOT is_deleted
            ORDER BY updated_at_utc DESC NULLS LAST, created_at_utc DESC
            LIMIT 1
            ON CONFLICT DO NOTHING;

            UPDATE reports.report_templates
            SET name = 'Cotización Pricing LCL - Cliente',
                description = 'Plantilla exclusiva para cotizaciones LCL. Presenta el embarque por CBM y no expone la naviera utilizada por el consolidado.',
                is_active = TRUE,
                updated_at_utc = NOW()
            WHERE code = 'pricing-lcl-client-quote'
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM reports.report_templates
            WHERE id = 'd153b5d1-cc7f-4ec0-9b8e-628e89e96117'::uuid
              AND code = 'pricing-lcl-client-quote';
            """
        );
    }
}
