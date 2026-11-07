using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class QuoteCarrierDetailsLayoutTests
{
    [TestMethod]
    public void CarrierDetails_ImmediatelyFollowTransit_AndAreOptional()
    {
        const string resource = "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";
        using var stream = typeof(AddCarrierDetailsToPricingQuotePdf).Assembly.GetManifestResourceStream(resource);
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();
        var transit = html.IndexOf("{{rate.transitTime}}", StringComparison.Ordinal);
        var route = html.IndexOf("{{rate.carrierRoute}}", StringComparison.Ordinal);
        var comments = html.IndexOf("{{rate.carrierComments}}", StringComparison.Ordinal);
        var summary = html.IndexOf("class=\"summary-grid\"", StringComparison.Ordinal);
        Assert.IsTrue(transit >= 0 && route > transit && comments > route && summary > comments);
        StringAssert.Contains(html, "{{#if rate.carrierRoute}}");
        StringAssert.Contains(html, "{{#if rate.carrierComments}}");
        StringAssert.Contains(html, "white-space:pre-wrap");
    }
}
