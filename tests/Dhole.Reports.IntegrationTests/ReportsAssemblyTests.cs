using Dhole.Reports.Infrastructure.Generation;
using Dhole.Reports.Persistence.DbContexts;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class ReportsAssemblyTests
{
    [TestMethod]
    public void FclAndLclPdfTemplate_DisplaysRevisionBesideQuoteNumber()
    {
        var assembly = typeof(ServiceDbContext).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "Dhole.Reports.Persistence.Templates.PricingClientQuoteCastroFallas.html");
        Assert.IsNotNull(stream, "La plantilla de cotización debe estar embebida.");
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        StringAssert.Contains(html, "{{rate.quoteNumber}}");
        StringAssert.Contains(html, "REV {{rate.revisionNumber}}");
    }

    [TestMethod]
    public void ServiceAssemblies_AreLoadable()
    {
        Assert.AreEqual("Dhole.Reports.Infrastructure", typeof(ReportDocumentGenerator).Assembly.GetName().Name);
        Assert.AreEqual("Dhole.Reports.Persistence", typeof(ServiceDbContext).Assembly.GetName().Name);
    }
}
