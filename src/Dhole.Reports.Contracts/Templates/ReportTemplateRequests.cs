namespace Dhole.Reports.Contracts.Templates;

public sealed record CreateReportTemplateRequest(
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
    string PageSize = "A4",
    string Orientation = "Portrait");

public sealed record UpdateReportTemplateRequest(
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
    string PageSize = "A4",
    string Orientation = "Portrait");

public sealed record GenerateReportRequest(
    string Format,
    string DataJson,
    string? FileName = null,
    string? SheetName = null);
