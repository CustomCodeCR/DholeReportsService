using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Reports.Domain.Templates.Entities;

public sealed class ReportTemplate : SoftDeletableAggregateRoot<Guid>
{
    private ReportTemplate() { }

    private ReportTemplate(
        Guid id,
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? createdBy)
        : base(id)
    {
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        HtmlContent = NormalizeRequired(htmlContent, nameof(htmlContent));
        DesignerJson = NormalizeRequired(designerJson, nameof(designerJson));
        PageSize = NormalizePageSize(pageSize);
        Orientation = NormalizeOrientation(orientation);
        PreviewPdf = previewPdf.Length == 0
            ? throw new ArgumentException("La vista previa PDF es requerida.", nameof(previewPdf))
            : previewPdf;
        PreviewGeneratedAtUtc = DateTime.UtcNow;
        IsActive = true;
        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string HtmlContent { get; private set; } = string.Empty;
    public string DesignerJson { get; private set; } = "{}";
    public string PageSize { get; private set; } = "A4";
    public string Orientation { get; private set; } = "Portrait";
    public byte[] PreviewPdf { get; private set; } = [];
    public DateTime PreviewGeneratedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static ReportTemplate Create(
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? createdBy = null)
    {
        return new ReportTemplate(
            Guid.NewGuid(),
            name,
            description,
            htmlContent,
            designerJson,
            pageSize,
            orientation,
            previewPdf,
            createdBy);
    }

    public void Update(
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? updatedBy = null)
    {
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        HtmlContent = NormalizeRequired(htmlContent, nameof(htmlContent));
        DesignerJson = NormalizeRequired(designerJson, nameof(designerJson));
        PageSize = NormalizePageSize(pageSize);
        Orientation = NormalizeOrientation(orientation);
        PreviewPdf = previewPdf.Length == 0
            ? throw new ArgumentException("La vista previa PDF es requerida.", nameof(previewPdf))
            : previewPdf;
        PreviewGeneratedAtUtc = DateTime.UtcNow;
        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());
    }

    public void SetActive(bool isActive, Guid? updatedBy = null)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());
    }

    public void Delete(Guid? deletedBy = null)
    {
        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El valor es requerido.", parameterName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizePageSize(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "A4" : value.Trim().ToUpperInvariant();
        return normalized is "A4" or "LETTER" or "LEGAL" ? normalized : "A4";
    }

    private static string NormalizeOrientation(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "Portrait" : value.Trim();
        return normalized.Equals("landscape", StringComparison.OrdinalIgnoreCase)
            ? "Landscape"
            : "Portrait";
    }
}
