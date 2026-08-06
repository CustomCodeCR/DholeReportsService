using System.Text;
using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Reports.Domain.Templates.Entities;

public sealed class ReportTemplate : SoftDeletableAggregateRoot<Guid>
{
    private ReportTemplate() { }

    private ReportTemplate(
        Guid id,
        string code,
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string dataSchemaJson,
        string sampleDataJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? createdBy)
        : base(id)
    {
        Code = NormalizeTemplateCode(code, name);
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        HtmlContent = NormalizeRequired(htmlContent, nameof(htmlContent));
        DesignerJson = NormalizeJson(designerJson);
        DataSchemaJson = NormalizeJson(dataSchemaJson);
        SampleDataJson = NormalizeJson(sampleDataJson);
        PageSize = NormalizePageSize(pageSize);
        Orientation = NormalizeOrientation(orientation);
        PreviewPdf = previewPdf.Length == 0
            ? throw new ArgumentException("La vista previa PDF es requerida.", nameof(previewPdf))
            : previewPdf;
        PreviewGeneratedAtUtc = DateTime.UtcNow;
        IsActive = true;
        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string HtmlContent { get; private set; } = string.Empty;
    public string DesignerJson { get; private set; } = "{}";
    public string DataSchemaJson { get; private set; } = "{}";
    public string SampleDataJson { get; private set; } = "{}";
    public string PageSize { get; private set; } = "A4";
    public string Orientation { get; private set; } = "Portrait";
    public byte[] PreviewPdf { get; private set; } = [];
    public DateTime PreviewGeneratedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// Mantiene compatibilidad con la firma original de creación de plantillas.
    /// El código se genera a partir del nombre y los metadatos nuevos se
    /// inicializan con objetos JSON vacíos.
    /// </summary>
    public static ReportTemplate Create(
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? createdBy = null) =>
        Create(
            code: null,
            name: name,
            description: description,
            htmlContent: htmlContent,
            designerJson: designerJson,
            dataSchemaJson: "{}",
            sampleDataJson: "{}",
            pageSize: pageSize,
            orientation: orientation,
            previewPdf: previewPdf,
            createdBy: createdBy);

    public static ReportTemplate Create(
        string? code,
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string dataSchemaJson,
        string sampleDataJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? createdBy = null) =>
        new(
            Guid.NewGuid(),
            code ?? string.Empty,
            name,
            description,
            htmlContent,
            designerJson,
            dataSchemaJson,
            sampleDataJson,
            pageSize,
            orientation,
            previewPdf,
            createdBy);

    /// <summary>
    /// Mantiene compatibilidad con la firma original de actualización.
    /// Conserva el código y los metadatos de datos ya almacenados.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? updatedBy = null) =>
        Update(
            code: Code,
            name: name,
            description: description,
            htmlContent: htmlContent,
            designerJson: designerJson,
            dataSchemaJson: DataSchemaJson,
            sampleDataJson: SampleDataJson,
            pageSize: pageSize,
            orientation: orientation,
            previewPdf: previewPdf,
            updatedBy: updatedBy);

    public void Update(
        string? code,
        string name,
        string? description,
        string htmlContent,
        string designerJson,
        string dataSchemaJson,
        string sampleDataJson,
        string pageSize,
        string orientation,
        byte[] previewPdf,
        Guid? updatedBy = null)
    {
        Code = NormalizeTemplateCode(code ?? Code, name);
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        HtmlContent = NormalizeRequired(htmlContent, nameof(htmlContent));
        DesignerJson = NormalizeJson(designerJson);
        DataSchemaJson = NormalizeJson(dataSchemaJson);
        SampleDataJson = NormalizeJson(sampleDataJson);
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

    public void Delete(Guid? deletedBy = null) =>
        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El valor es requerido.", parameterName);
        return value.Trim();
    }

    private static string NormalizeJson(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeTemplateCode(string? value, string name)
    {
        var source = string.IsNullOrWhiteSpace(value) ? name : value;
        var builder = new StringBuilder(source.Length);
        var previousWasDash = false;

        foreach (var character in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
            }
            else if (!previousWasDash && builder.Length > 0)
            {
                builder.Append('-');
                previousWasDash = true;
            }
        }

        var normalized = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("El código de la plantilla no es válido.", nameof(value));

        return normalized.Length <= 150 ? normalized : normalized[..150].TrimEnd('-');
    }

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
