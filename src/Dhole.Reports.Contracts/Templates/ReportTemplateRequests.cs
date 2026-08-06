using System.Text.Json.Serialization;
using Dhole.Reports.Contracts.Serialization;

namespace Dhole.Reports.Contracts.Templates;

public sealed record CreateReportTemplateRequest(
    string Name,
    string? Description,
    string HtmlContent,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string DesignerJson,
    string PageSize = "A4",
    string Orientation = "Portrait",
    string? Code = null,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string DataSchemaJson = "{}",
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string SampleDataJson = "{}");

public sealed record UpdateReportTemplateRequest(
    string Name,
    string? Description,
    string HtmlContent,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string DesignerJson,
    string PageSize = "A4",
    string Orientation = "Portrait",
    string? Code = null,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string DataSchemaJson = "{}",
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string SampleDataJson = "{}");

public sealed record GenerateReportRequest(
    string Format,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string DataJson,
    string? FileName = null,
    string? SheetName = null);

public sealed record RenderReportTemplatePreviewRequest(
    string HtmlContent,
    [property: JsonConverter(typeof(FlexibleJsonStringConverter))] string SampleDataJson,
    string PageSize = "A4",
    string Orientation = "Portrait");
