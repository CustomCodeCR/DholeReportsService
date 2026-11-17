using System.Reflection;
using System.Text.Json;
using Dhole.Reports.Infrastructure.Generation;
using Dhole.Reports.Persistence.Migrations;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class QuoteApproximateTransitTests
{
    [TestMethod]
    public void EmbeddedTemplate_ShowsApproximateTransitHeading()
    {
        const string name = "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html";
        using var stream = typeof(ApproximateTransitTimeInPricingQuotes).Assembly.GetManifestResourceStream(name);
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        StringAssert.Contains(html, "Tiempo aproximado de tránsito");
        StringAssert.Contains(html, "{{rate.transitTime}}");
        Assert.IsFalse(html.Contains("Tiempo de tránsito</div>", StringComparison.Ordinal));
    }

    [DataTestMethod]
    [DataRow("38 días", "38 días aprox.")]
    [DataRow("38", "38 días aprox.")]
    [DataRow("38 a 42 días", "38 a 42 días aprox.")]
    [DataRow("38 días aprox.", "38 días aprox.")]
    [DataRow("Por confirmar", "Por confirmar")]
    [DataRow("No aplica", "No aplica")]
    public void AllPricingPdfTemplates_AppendApproxOnlyWhenApplicable(string value, string expected)
    {
        var binder = typeof(PricingQuoteGroupHtmlBuilder).Assembly
            .GetType("Dhole.Reports.Infrastructure.Generation.TemplateDataBinder");
        Assert.IsNotNull(binder);
        var method = binder.GetMethod("Bind", BindingFlags.Static | BindingFlags.Public);
        Assert.IsNotNull(method);

        var data = JsonSerializer.Serialize(new { rate = new { transitTime = value } });
        var rendered = method.Invoke(null, ["<div>{{rate.transitTime}}</div>", data]) as string;
        Assert.AreEqual("<div>" + expected + "</div>", rendered);
    }
}
