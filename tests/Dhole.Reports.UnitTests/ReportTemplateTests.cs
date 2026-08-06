using Dhole.Reports.Domain.Templates.Entities;

namespace Dhole.Reports.UnitTests;

[TestClass]
public sealed class ReportTemplateTests
{
    [TestMethod]
    public void Create_NormalizesConfigurationAndStoresPreview()
    {
        var template = ReportTemplate.Create(
            "  Tarifa marítima  ",
            "  Plantilla comercial  ",
            "<html><body>{{title}}</body></html>",
            "{\"version\":1,\"blocks\":[]}",
            "letter",
            "landscape",
            [1, 2, 3],
            Guid.NewGuid());

        Assert.AreEqual("Tarifa marítima", template.Name);
        Assert.AreEqual("Plantilla comercial", template.Description);
        Assert.AreEqual("LETTER", template.PageSize);
        Assert.AreEqual("Landscape", template.Orientation);
        Assert.HasCount(3, template.PreviewPdf);
        Assert.IsTrue(template.IsActive);
        Assert.IsFalse(template.IsDeleted);
    }

    [TestMethod]
    public void Delete_AppliesSoftDeleteMetadata()
    {
        var deletedBy = Guid.NewGuid();
        var template = ReportTemplate.Create(
            "Reporte",
            null,
            "<html><body>Reporte</body></html>",
            "{\"version\":1,\"blocks\":[]}",
            "A4",
            "Portrait",
            [1]);

        template.Delete(deletedBy);

        Assert.IsTrue(template.IsDeleted);
        Assert.IsNotNull(template.DeletedAtUtc);
        Assert.AreEqual(deletedBy.ToString(), template.DeletedBy);
    }
}
