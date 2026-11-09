using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Reports.Persistence.Migrations;

/// <summary>
/// Additive F6.06 migration; no report history is deleted or rewritten.
/// Applied only during the coordinated release migration stage.
/// </summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261108164500_AddReportsRealtimeOutbox")]
public sealed class AddReportsRealtimeOutbox : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "reports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                error_message = table.Column<string>(type: "text", nullable: true),
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                headers_json = table.Column<string>(type: "jsonb", nullable: true),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                retry_count = table.Column<int>(type: "integer", nullable: false),
                source_service = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table => table.PrimaryKey("p_k_outbox_messages", x => x.id));
        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_event_id",
            schema: "reports", table: "outbox_messages", column: "event_id", unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_status_created_at",
            schema: "reports", table: "outbox_messages",
            columns: new[] { "status", "created_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Production runbook requires archiving pending events before rollback.
        migrationBuilder.DropTable(name: "outbox_messages", schema: "reports");
    }
}
