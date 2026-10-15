using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class SelectedTaxQuoteTemplateTests
{
    private const string HtmlResource =
        "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";

    [TestMethod]
    public void PricingQuoteTemplate_ShowsTaxOnlyWhenSelectedInPricing()
    {
        using var stream = typeof(ShowSelectedDestinationTaxInPricingQuote)
            .Assembly.GetManifestResourceStream(HtmlResource);
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        StringAssert.Contains(html, "{{#if rate.hasDestinationTax}}");
        StringAssert.Contains(html, "{{tax}}");
        StringAssert.Contains(html, "{{subtotal}}");
        StringAssert.Contains(html, "{{#each currencyTotals}}");
        StringAssert.Contains(html, "{{destinationTaxLabel}}");

        var conditionalOpen = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{#if ").Count;
        var conditionalClose = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{/if\}\}").Count;
        Assert.AreEqual(conditionalOpen, conditionalClose);
    }
}
