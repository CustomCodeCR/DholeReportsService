using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260913220000_AddContentAnalytics")]
public sealed class AddContentAnalytics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "content_analytics_events",
            schema: "reports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                event_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                site_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                content_id = table.Column<Guid>(type: "uuid", nullable: true),
                form_id = table.Column<Guid>(type: "uuid", nullable: true),
                submission_id = table.Column<Guid>(type: "uuid", nullable: true),
                meeting_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                placement_id = table.Column<Guid>(type: "uuid", nullable: true),
                path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                interaction_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                target_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                source_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                referrer_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                utm_source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                utm_medium = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                utm_campaign = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                utm_content = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                utm_term = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_content_analytics_events", x => x.id));

        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_site_key_occurred_at_utc", schema: "reports", table: "content_analytics_events", columns: new[] { "site_key", "occurred_at_utc" });
        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_event_name_occurred_at_utc", schema: "reports", table: "content_analytics_events", columns: new[] { "event_name", "occurred_at_utc" });
        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_content_id", schema: "reports", table: "content_analytics_events", column: "content_id");
        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_campaign_id", schema: "reports", table: "content_analytics_events", column: "campaign_id");
        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_form_id", schema: "reports", table: "content_analytics_events", column: "form_id");
        migrationBuilder.CreateIndex(name: "IX_content_analytics_events_placement_id", schema: "reports", table: "content_analytics_events", column: "placement_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "content_analytics_events", schema: "reports");
}
