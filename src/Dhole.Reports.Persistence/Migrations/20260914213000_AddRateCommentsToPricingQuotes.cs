using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260914213000_AddRateCommentsToPricingQuotes")]
public sealed class AddRateCommentsToPricingQuotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    $rate_comments_anchor${{#if rate.excludes}}<div class="terms-card"><h3>No incluye</h3><p>{{rate.excludes}}</p></div>{{/if}}$rate_comments_anchor$,
                    $rate_comments_block${{#if rate.excludes}}<div class="terms-card"><h3>No incluye</h3><p>{{rate.excludes}}</p></div>{{/if}}
                {{#if rate.comments}}<div class="terms-card neutral"><h3>Comentarios de la tarifa</h3><p>{{rate.comments}}</p></div>{{/if}}$rate_comments_block$
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted
              AND POSITION('{{#if rate.comments}}' IN html_content) = 0;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE reports.report_templates
            SET html_content = REPLACE(
                    html_content,
                    $rate_comments_block${{#if rate.excludes}}<div class="terms-card"><h3>No incluye</h3><p>{{rate.excludes}}</p></div>{{/if}}
                {{#if rate.comments}}<div class="terms-card neutral"><h3>Comentarios de la tarifa</h3><p>{{rate.comments}}</p></div>{{/if}}$rate_comments_block$,
                    $rate_comments_anchor${{#if rate.excludes}}<div class="terms-card"><h3>No incluye</h3><p>{{rate.excludes}}</p></div>{{/if}}$rate_comments_anchor$
                ),
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }
}
