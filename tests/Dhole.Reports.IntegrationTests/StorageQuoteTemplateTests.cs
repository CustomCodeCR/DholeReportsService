using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class StorageQuoteTemplateTests
{
    private static readonly string HtmlResource =
        "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";

    [TestMethod]
    public void QuoteTemplate_ShowsStorageOnlyWhenServiceIsPresent()
    {
        var assembly = typeof(ShowFiscalStorageInPricingQuote).Assembly;
        using var stream = assembly.GetManifestResourceStream(HtmlResource);
        Assert.IsNotNull(stream, "La plantilla HTML debe seguir embebida en ReportsService.");
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        StringAssert.Contains(html, "{{#if rate.hasStorageService}}");
        StringAssert.Contains(html, "Días de almacenaje fiscal");
        StringAssert.Contains(html, "{{rate.storageDaysLabel}}");
        StringAssert.Contains(html, "{{rate.servicesSummary}}");
        StringAssert.Contains(html, "{{rate.documentTitle}}");
        StringAssert.Contains(html, "{{#if rate.hasTransportService}}");
        StringAssert.Contains(html, "{{rate.transitTime}}");
        StringAssert.Contains(html, "{{#each items}}");
        StringAssert.Contains(html, "{{rate.revisionNumber}}");

        Assert.IsFalse(html.Contains(
            "No incluye demoras, almacenajes, impuestos, permisos ni servicios no indicados expresamente.",
            StringComparison.Ordinal));
    }

    [TestMethod]
    public void QuoteTemplate_ConditionsAreBalanced()
    {
        var assembly = typeof(ShowFiscalStorageInPricingQuote).Assembly;
        using var stream = assembly.GetManifestResourceStream(HtmlResource);
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();
        var opening = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{#if ").Count;
        var closing = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{/if\}\}").Count;
        Assert.AreEqual(opening, closing, "Los bloques condicionales de la cotización deben balancearse.");
    }
}
