using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Dhole.Reports.Persistence.DbContexts;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260804100000_InitialReports")]
public sealed class InitialReports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "reports");

        migrationBuilder.CreateTable(
            name: "report_templates",
            schema: "reports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                html_content = table.Column<string>(type: "text", nullable: false),
                designer_json = table.Column<string>(type: "jsonb", nullable: false),
                page_size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                orientation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                preview_pdf = table.Column<byte[]>(type: "bytea", nullable: false),
                preview_generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("p_k_report_templates", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_report_templates_name",
            schema: "reports",
            table: "report_templates",
            column: "name",
            unique: true,
            filter: "NOT is_deleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "report_templates", schema: "reports");
    }
}
