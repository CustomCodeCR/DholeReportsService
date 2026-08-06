using CustomCodeFramework.Core.Results;

namespace Dhole.Reports.Domain.Shared;

public static class ReportsErrors
{
    public static readonly Error TemplateNotFound = new(
        "Reports.Template.NotFound",
        "La plantilla de reporte no existe.");

    public static readonly Error TemplateNameAlreadyExists = new(
        "Reports.Template.NameAlreadyExists",
        "Ya existe una plantilla con ese nombre.");

    public static readonly Error TemplateCodeAlreadyExists = new(
        "Reports.Template.CodeAlreadyExists",
        "Ya existe una plantilla con ese código.");

    public static readonly Error InvalidTemplate = new(
        "Reports.Template.Invalid",
        "La plantilla HTML, el JSON del diseñador, el esquema o los datos de muestra no son válidos.");

    public static readonly Error InvalidReportData = new(
        "Reports.Generation.InvalidData",
        "Los datos del reporte deben ser un JSON válido.");

    public static readonly Error UnsupportedFormat = new(
        "Reports.Generation.UnsupportedFormat",
        "El formato solicitado no es soportado. Use pdf, xlsx o csv.");

    public static readonly Error GenerationFailed = new(
        "Reports.Generation.Failed",
        "No fue posible generar el reporte.");
}
