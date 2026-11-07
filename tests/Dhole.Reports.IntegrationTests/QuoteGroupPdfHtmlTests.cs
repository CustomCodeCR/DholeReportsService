using Dhole.Reports.Infrastructure.Generation;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class QuoteGroupPdfHtmlTests
{
    private const string Parent = "QUO-00000-000600";
    private const string Template = "<!doctype html><html><head><style>.page{font-weight:bold}</style></head>" +
        "<body><section class=\"page\">{{rate.quoteNumber}} | {{rate.carrier}} | {{rate.total}}</section></body></html>";

    private static GroupedPricingQuoteRenderItem Option(string suffix, string carrier, string price) => new(
        Parent + "-" + suffix,
        "Shanghai → " + (suffix == "A" ? "Caldera" : "Moín"),
        carrier, price, "USD", Template,
        "{\"rate\":{\"quoteNumber\":\"" + Parent + "-" + suffix + "\",\"carrier\":\"" + carrier +
        "\",\"total\":\"USD " + price + "\"}}");

    [TestMethod]
    public void GeneratesOneDocumentWithIndexAndAnchorsToOriginalQuoteBodies()
    {
        var html = PricingQuoteGroupHtmlBuilder.Build(Parent,
            [Option("A", "MSC", "6800"), Option("B", "MAERSK", "6000")]);
        StringAssert.Contains(html, "href=\"#quo-option-A\"");
        StringAssert.Contains(html, "href=\"#quo-option-B\"");
        StringAssert.Contains(html, "id=\"quo-option-A\"");
        StringAssert.Contains(html, "id=\"quo-option-B\"");
        StringAssert.Contains(html, "Volver al índice");
        StringAssert.Contains(html, "USD 6800");
        StringAssert.Contains(html, "USD 6000");
        StringAssert.Contains(html, Parent + "-A | MSC | USD 6800");
        StringAssert.Contains(html, Parent + "-B | MAERSK | USD 6000");
        Assert.AreEqual(1, html.Split("<!DOCTYPE html>", StringSplitOptions.None).Length - 1);
        Assert.AreEqual(1, html.Split("</html>", StringSplitOptions.None).Length - 1);
    }

    [TestMethod]
    public void EscapesIndexTextWithoutDroppingCommercialTemplateFormatting()
    {
        var unsafeOption = Option("A", "<script>alert(1)</script>", "6800");
        var html = PricingQuoteGroupHtmlBuilder.Build(Parent,
            [unsafeOption, Option("B", "MSC", "6000")]);
        StringAssert.Contains(html, "&lt;script&gt;");
        StringAssert.Contains(html, ".page{font-weight:bold}");
        Assert.IsFalse(html.Contains("<td><script>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RejectsDuplicateOrUnrelatedSubQuotes()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            PricingQuoteGroupHtmlBuilder.Build(Parent, [Option("A", "MSC", "1"), Option("A", "MSC", "1")]));
        Assert.ThrowsExactly<ArgumentException>(() =>
            PricingQuoteGroupHtmlBuilder.Build(Parent, [Option("A", "MSC", "1")]));
        Assert.ThrowsExactly<ArgumentException>(() =>
            PricingQuoteGroupHtmlBuilder.Build("QUO-X", [Option("A", "MSC", "1"), Option("B", "MSC", "1")]));
    }

    [TestMethod]
    public void ThirtyAlternatives_HaveThirtyWorkingIndexTargets()
    {
        var options = Enumerable.Range(0, 30)
            .Select(i => Option(Suffix(i), "MSC", i.ToString()))
            .ToArray();
        var html = PricingQuoteGroupHtmlBuilder.Build(Parent, options);
        Assert.AreEqual(30, html.Split("class=\"quote-group-option\"", StringSplitOptions.None).Length - 1);
        StringAssert.Contains(html, "href=\"#quo-option-AD\"");
        StringAssert.Contains(html, "id=\"quo-option-AD\"");
    }

    private static string Suffix(int index)
    {
        var result = string.Empty;
        do
        {
            result = (char)('A' + index % 26) + result;
            index = index / 26 - 1;
        } while (index >= 0);
        return result;
    }
}
