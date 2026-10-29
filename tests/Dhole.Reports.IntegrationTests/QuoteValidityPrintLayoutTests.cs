using System.Text.RegularExpressions;
using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class QuoteValidityPrintLayoutTests
{
    private const string TemplateResource =
        "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";

    private static string ReadTemplate()
    {
        using var stream = typeof(EmphasizeQuoteValidityOnPrintedPdf).Assembly
            .GetManifestResourceStream(TemplateResource);
        Assert.IsNotNull(stream, "Debe existir la plantilla PDF de Castro Fallas.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [TestMethod]
    public void QuoteValidity_AppearsImmediatelyBelowClientInSummary()
    {
        var html = ReadTemplate();
        var clientRow = "<tr><td class=\"k\">Cliente</td><td class=\"v\">{{rate.clientName}}</td></tr>";
        var validityRow = "<tr class=\"validity-detail\"><td class=\"k\">Vigencia</td><td class=\"v\">{{rate.validFrom}} al {{rate.validTo}}</td></tr>";

        var clientIndex = html.IndexOf(clientRow, StringComparison.Ordinal);
        var validityIndex = html.IndexOf(validityRow, StringComparison.Ordinal);
        Assert.IsTrue(clientIndex >= 0 && validityIndex > clientIndex);
        Assert.IsTrue(
            string.IsNullOrWhiteSpace(html[(clientIndex + clientRow.Length)..validityIndex]),
            "Vigencia debe ubicarse inmediatamente después del cliente.");
        Assert.AreEqual(1, Regex.Matches(html, "class=\"validity-detail\"").Count);
    }

    [TestMethod]
    public void QuoteValidity_IsBoldAndReadableInPrintedRedBar()
    {
        var html = ReadTemplate();
        StringAssert.Contains(html,
            "<span class=\"validity-highlight\">VIGENCIA: {{rate.validFrom}} AL {{rate.validTo}}</span>");
        StringAssert.Contains(html, ".accent-strip .validity-highlight");
        StringAssert.Contains(html, "font-weight:900");
        StringAssert.Contains(html, "border-bottom:2px solid #fff");
        StringAssert.Contains(html, "@media print");
        StringAssert.Contains(html, "-webkit-print-color-adjust:exact");
        StringAssert.Contains(html, "print-color-adjust:exact");
        Assert.AreEqual(1, Regex.Matches(html, "class=\"validity-highlight\"").Count);
    }

    [TestMethod]
    public void QuoteValidity_DoesNotRemoveOtherQuotationFields()
    {
        var html = ReadTemplate();
        StringAssert.Contains(html, "{{rate.quoteNumber}}");
        StringAssert.Contains(html, "{{rate.clientName}}");
        StringAssert.Contains(html, "{{rate.idtraNumber}}");
        StringAssert.Contains(html, "{{rate.validFrom}}");
        StringAssert.Contains(html, "{{rate.validTo}}");
        StringAssert.Contains(html, "{{#each items}}");
        StringAssert.Contains(html, "{{#each currencyTotals}}");
        StringAssert.Contains(html, "Vigencia de la oferta:");
    }
}
