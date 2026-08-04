namespace Dhole.Reports.Contracts.Templates;

public sealed record ReportTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
    string PageSize,
    string Orientation,
    bool HasPreviewPdf,
    DateTime PreviewGeneratedAtUtc,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record ReportTemplateListDto(
    Guid Id,
    string Name,
    string? Description,
    string PageSize,
    string Orientation,
    bool HasPreviewPdf,
    DateTime PreviewGeneratedAtUtc,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
