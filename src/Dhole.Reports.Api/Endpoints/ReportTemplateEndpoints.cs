using System.Security.Cryptography;
using System.Text;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Reports.Api.Authorization;
using Dhole.Reports.Api.Extensions;
using Dhole.Reports.Application.Templates;
using Dhole.Reports.Contracts.Templates;

namespace Dhole.Reports.Api.Endpoints;

public static class ReportTemplateEndpoints
{
    public static IEndpointRouteBuilder MapReportTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports/templates")
            .WithTags("Report Templates")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? pageNumber,
            int? pageSize,
            string? search,
            bool? isActive,
            IQueryDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new BrowseReportTemplatesQuery(
                    PageRequest.Create(Math.Max(1, pageNumber ?? 1), Math.Clamp(pageSize ?? 25, 1, 100)),
                    search,
                    isActive),
                cancellationToken);
            return EndpointResults.FromPaged(result);
        }).RequireScope(ReportsScopeNames.TemplatesView);

        group.MapGet("/by-code/{code}", async (
            string code,
            IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(new GetReportTemplateByCodeQuery(code), cancellationToken);
            return EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesView);

        group.MapGet("/{templateId:guid}", async (
            Guid templateId,
            IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(new GetReportTemplateByIdQuery(templateId), cancellationToken);
            return EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesView);

        group.MapGet("/{templateId:guid}/preview.pdf", async (
            Guid templateId,
            IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(new GetReportTemplatePreviewQuery(templateId), cancellationToken);
            return result.IsSuccess
                ? Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
                : EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesView);


        group.MapPost("/render-preview", async (
            RenderReportTemplatePreviewRequest request,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new RenderReportTemplatePreviewCommand(
                    request.HtmlContent,
                    request.SampleDataJson,
                    request.PageSize,
                    request.Orientation),
                cancellationToken);
            return ToFileOrError(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesView);

        group.MapPost("/", async (
            CreateReportTemplateRequest request,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new CreateReportTemplateCommand(
                    request.Code,
                    request.Name,
                    request.Description,
                    request.HtmlContent,
                    request.DesignerJson,
                    request.DataSchemaJson,
                    request.SampleDataJson,
                    request.PageSize,
                    request.Orientation,
                    context.GetCurrentUserId()),
                cancellationToken);
            return EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesCreate);

        group.MapPut("/{templateId:guid}", async (
            Guid templateId,
            UpdateReportTemplateRequest request,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new UpdateReportTemplateCommand(
                    templateId,
                    request.Code,
                    request.Name,
                    request.Description,
                    request.HtmlContent,
                    request.DesignerJson,
                    request.DataSchemaJson,
                    request.SampleDataJson,
                    request.PageSize,
                    request.Orientation,
                    context.GetCurrentUserId()),
                cancellationToken);
            return EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesUpdate);

        group.MapDelete("/{templateId:guid}", async (
            Guid templateId,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new DeleteReportTemplateCommand(templateId, context.GetCurrentUserId()), cancellationToken);
            return EndpointResults.FromResult(result, context);
        }).RequireScope(ReportsScopeNames.TemplatesDelete);

        group.MapPost("/{templateId:guid}/generate", async (
            Guid templateId,
            GenerateReportRequest request,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new GenerateReportCommand(templateId, request.Format, request.DataJson, request.FileName, request.SheetName),
                cancellationToken);
            return ToFileOrError(result, context);
        }).RequireScope(ReportsScopeNames.ReportsGenerate);

        group.MapPost("/by-code/{code}/generate", async (
            string code,
            GenerateReportRequest request,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(
                new GenerateReportByCodeCommand(code, request.Format, request.DataJson, request.FileName, request.SheetName),
                cancellationToken);
            return ToFileOrError(result, context);
        }).RequireScope(ReportsScopeNames.ReportsGenerate);

        app.MapPost("/api/internal/reports/templates/{code}/generate", async (
            string code,
            GenerateReportRequest request,
            IConfiguration configuration,
            ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (!HasValidServiceKey(context, configuration))
                return Results.Unauthorized();

            var result = await dispatcher.DispatchAsync(
                new GenerateReportByCodeCommand(code, request.Format, request.DataJson, request.FileName, request.SheetName),
                cancellationToken);
            return ToFileOrError(result, context);
        })
        .WithTags("Internal Reports")
        .AllowAnonymous();

        return app;
    }

    private static IResult ToFileOrError(
        CustomCodeFramework.Core.Results.Result<Dhole.Reports.Contracts.Generation.GeneratedReportDto> result,
        HttpContext context) =>
        result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : EndpointResults.FromResult(result, context);

    private static bool HasValidServiceKey(HttpContext context, IConfiguration configuration)
    {
        var expected = configuration["Reports:InternalServiceKey"];
        var headerName = configuration["Reports:InternalServiceKeyHeader"] ?? "X-Dhole-Service-Key";
        var provided = context.Request.Headers[headerName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(provided))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
