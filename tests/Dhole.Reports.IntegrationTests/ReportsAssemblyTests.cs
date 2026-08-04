using Dhole.Reports.Infrastructure.Generation;
using Dhole.Reports.Persistence.DbContexts;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class ReportsAssemblyTests
{
    [TestMethod]
    public void ServiceAssemblies_AreLoadable()
    {
        Assert.AreEqual("Dhole.Reports.Infrastructure", typeof(ReportDocumentGenerator).Assembly.GetName().Name);
        Assert.AreEqual("Dhole.Reports.Persistence", typeof(ServiceDbContext).Assembly.GetName().Name);
    }
}
