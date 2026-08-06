using System.Text.Json;
using Dhole.Reports.Application.Abstractions.Generation;
using Dhole.Reports.Contracts.Generation;

namespace Dhole.Reports.Api.Endpoints;

public static class TabularReportEndpoints
{
    public static IEndpointRouteBuilder MapTabularReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/reports/tabular")
            .WithTags("Tabular Reports")
            .RequireAuthorization();

        group.MapPost("/generate", GenerateAsync);

        return app;
    }

    private static async Task<IResult> GenerateAsync(
        GenerateTabularReportRequest request,
        IReportDocumentGenerator generator,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var format = request.Format?.Trim().ToLowerInvariant();
        if (format is not ("xlsx" or "csv"))
        {
            return BadRequest(
                httpContext,
                "Reports.UnsupportedTabularFormat",
                "El formato debe ser xlsx o csv."
            );
        }

        if (string.IsNullOrWhiteSpace(request.DataJson))
        {
            return BadRequest(
                httpContext,
                "Reports.EmptyTabularData",
                "Debe enviar datos tabulares en DataJson."
            );
        }

        try
        {
            using var _ = JsonDocument.Parse(request.DataJson);
        }
        catch (JsonException)
        {
            return BadRequest(
                httpContext,
                "Reports.InvalidTabularData",
                "DataJson no contiene JSON válido."
            );
        }

        try
        {
            var generated = await generator.GenerateAsync(
                format,
                string.Empty,
                request.DataJson,
                string.IsNullOrWhiteSpace(request.FileName)
                    ? "resultado-ia"
                    : request.FileName.Trim(),
                "A4",
                "Portrait",
                request.SheetName,
                cancellationToken
            );

            return Results.File(
                generated.Content,
                generated.ContentType,
                generated.FileName
            );
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return BadRequest(
                httpContext,
                "Reports.TabularGenerationFailed",
                $"No fue posible generar el archivo: {exception.Message}"
            );
        }
    }

    private static IResult BadRequest(
        HttpContext httpContext,
        string code,
        string message
    )
    {
        return Results.BadRequest(
            new
            {
                title = "Tabular report error",
                status = StatusCodes.Status400BadRequest,
                detail = message,
                instance = httpContext.Request.Path.Value,
                traceId = httpContext.TraceIdentifier,
                code,
            }
        );
    }
}
