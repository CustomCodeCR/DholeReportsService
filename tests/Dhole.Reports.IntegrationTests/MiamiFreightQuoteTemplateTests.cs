using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class MiamiFreightQuoteTemplateTests
{
    [TestMethod]
    public void ClientQuoteTemplate_HasOnlyAnOptionalNonAdditiveMiamiBreakdown()
    {
        const string resource = "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";
        using var stream = typeof(ShowMiamiFreightCostBreakdownOnPdf).Assembly
            .GetManifestResourceStream(resource);
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        StringAssert.Contains(html, "<span class=\"miami-matrix-note\">{{miamiBreakdown}}</span>");
        StringAssert.Contains(html, "white-space:pre-line");
        StringAssert.Contains(html, ".miami-matrix-note:empty { display:none; }");
        // The detail is informational, displayed INSIDE the existing item cell.
        // The payable amount and per-currency totals still use the saved sale.
        StringAssert.Contains(html, "{{lineTotalWithTaxAmount}}");
        StringAssert.Contains(html, "{{#each currencyTotals}}");
        Assert.AreEqual(1, html.Split("<span class=\"miami-matrix-note\">").Length - 1);
    }
}
