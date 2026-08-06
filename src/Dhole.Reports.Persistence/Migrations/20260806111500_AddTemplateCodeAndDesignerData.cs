using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260806111500_AddTemplateCodeAndDesignerData")]
public sealed class AddTemplateCodeAndDesignerData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "code",
            schema: "reports",
            table: "report_templates",
            type: "character varying(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "data_schema_json",
            schema: "reports",
            table: "report_templates",
            type: "jsonb",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<string>(
            name: "sample_data_json",
            schema: "reports",
            table: "report_templates",
            type: "jsonb",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.Sql("""
            UPDATE reports.report_templates
            SET code = 'template-' || substring(id::text, 1, 8)
            WHERE code IS NULL OR btrim(code) = '';
            """);

        migrationBuilder.AlterColumn<string>(
            name: "code",
            schema: "reports",
            table: "report_templates",
            type: "character varying(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(150)",
            oldMaxLength: 150,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_report_templates_code",
            schema: "reports",
            table: "report_templates",
            column: "code",
            unique: true,
            filter: "NOT is_deleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_report_templates_code",
            schema: "reports",
            table: "report_templates");

        migrationBuilder.DropColumn(name: "code", schema: "reports", table: "report_templates");
        migrationBuilder.DropColumn(name: "data_schema_json", schema: "reports", table: "report_templates");
        migrationBuilder.DropColumn(name: "sample_data_json", schema: "reports", table: "report_templates");
    }
}
