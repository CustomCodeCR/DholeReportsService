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

        // IVA is displayed in dedicated invoice columns, not as a red annotation
        // under each concept or as repeated totals above the final amount.
        StringAssert.Contains(html, "{{#if rate.hasDestinationTax}}");
        StringAssert.Contains(html, "items-table--with-tax");
        StringAssert.Contains(html, "<th class=\"money tax\">IVA</th>");
        StringAssert.Contains(html, "{{lineTaxAmount}}");
        StringAssert.Contains(html, "{{lineTotalWithTaxAmount}}");
        StringAssert.Contains(html, "{{subtotalAmount}}");
        StringAssert.Contains(html, "{{taxAmount}}");
        StringAssert.Contains(html, "{{amount}}");
        StringAssert.Contains(html, "{{#each currencyTotals}}");
        StringAssert.Contains(html, ".page-two { page-break-before: auto");
        Assert.IsFalse(html.Contains("{{destinationTaxLabel}}", StringComparison.Ordinal));
        Assert.IsFalse(html.Contains("page-break-before: always", StringComparison.Ordinal));

        var conditionalOpen = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{#if ").Count;
        var conditionalClose = System.Text.RegularExpressions.Regex.Matches(html, @"\{\{/if\}\}").Count;
        Assert.AreEqual(conditionalOpen, conditionalClose);
    }
}
