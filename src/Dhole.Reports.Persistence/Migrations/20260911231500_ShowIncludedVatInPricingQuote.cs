using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Reports.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260911231500_ShowIncludedVatInPricingQuote")]
public sealed class ShowIncludedVatInPricingQuote : Migration
{
    private const string HtmlResource =
        "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";
    private const string LogoResource =
        "Dhole.Reports.Persistence.Templates.CastroFallasLogo.png";
    private const string LogoPlaceholder = "__CASTRO_FALLAS_LOGO_DATA_URI__";
    private const string SqlDelimiter = "$castro_fallas_quote_vat$";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var assembly = typeof(ShowIncludedVatInPricingQuote).Assembly;

        using var htmlStream = assembly.GetManifestResourceStream(HtmlResource)
            ?? throw new InvalidOperationException($"No se encontró el recurso HTML '{HtmlResource}'.");
        using var htmlReader = new StreamReader(htmlStream);
        var html = htmlReader.ReadToEnd();

        using var logoStream = assembly.GetManifestResourceStream(LogoResource)
            ?? throw new InvalidOperationException($"No se encontró el logo PNG '{LogoResource}'.");
        using var logoBuffer = new MemoryStream();
        logoStream.CopyTo(logoBuffer);
        var logoDataUri = $"data:image/png;base64,{Convert.ToBase64String(logoBuffer.ToArray())}";

        if (!html.Contains(LogoPlaceholder, StringComparison.Ordinal))
            throw new InvalidOperationException("La plantilla HTML no contiene el marcador del logo de Castro Fallas.");

        html = html.Replace(LogoPlaceholder, logoDataUri, StringComparison.Ordinal);

        if (html.Contains(SqlDelimiter, StringComparison.Ordinal))
            throw new InvalidOperationException("La plantilla HTML contiene el delimitador reservado de la migración.");

        migrationBuilder.Sql(
            $"""
            UPDATE reports.report_templates
            SET html_content = {SqlDelimiter}{html}{SqlDelimiter},
                updated_at_utc = NOW()
            WHERE code IN ('pricing-fcl-client-quote', 'pricing-lcl-client-quote')
              AND NOT is_deleted;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
