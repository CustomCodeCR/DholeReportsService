using System.Text.Json;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Reports.Application.Abstractions.Generation;
using Dhole.Reports.Application.Abstractions.Repositories;
using Dhole.Reports.Contracts.Generation;
using Dhole.Reports.Contracts.Templates;
using Dhole.Reports.Domain.Shared;
using Dhole.Reports.Domain.Templates.Entities;

namespace Dhole.Reports.Application.Templates;

public sealed record BrowseReportTemplatesQuery(
    PageRequest Page,
    string? Search,
    bool? IsActive) : IQuery<PagedResult<ReportTemplateListDto>>;

public sealed class BrowseReportTemplatesQueryHandler(IReportTemplateRepository templates)
    : IQueryHandler<BrowseReportTemplatesQuery, PagedResult<ReportTemplateListDto>>
{
    public Task<PagedResult<ReportTemplateListDto>> HandleAsync(
        BrowseReportTemplatesQuery query,
        CancellationToken cancellationToken = default) =>
        templates.GetPagedAsync(query.Page, query.Search, query.IsActive, cancellationToken);
}

public sealed record GetReportTemplateByIdQuery(Guid Id) : IQuery<Result<ReportTemplateDto>>;
public sealed record GetReportTemplateByCodeQuery(string Code) : IQuery<Result<ReportTemplateDto>>;

public sealed class GetReportTemplateByIdQueryHandler(IReportTemplateRepository templates)
    : IQueryHandler<GetReportTemplateByIdQuery, Result<ReportTemplateDto>>
{
    public async Task<Result<ReportTemplateDto>> HandleAsync(
        GetReportTemplateByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(query.Id, cancellationToken);
        return template is null || template.IsDeleted
            ? Result.Failure<ReportTemplateDto>(ReportsErrors.TemplateNotFound)
            : Result.Success(Map(template));
    }

    internal static ReportTemplateDto Map(ReportTemplate template) => new(
        template.Id,
        template.Code,
        template.Name,
        template.Description,
        template.HtmlContent,
        template.DesignerJson,
        template.DataSchemaJson,
        template.SampleDataJson,
        template.PageSize,
        template.Orientation,
        template.PreviewPdf.Length > 0,
        template.PreviewGeneratedAtUtc,
        template.IsActive,
        template.CreatedAtUtc,
        template.UpdatedAtUtc);
}

public sealed class GetReportTemplateByCodeQueryHandler(IReportTemplateRepository templates)
    : IQueryHandler<GetReportTemplateByCodeQuery, Result<ReportTemplateDto>>
{
    public async Task<Result<ReportTemplateDto>> HandleAsync(
        GetReportTemplateByCodeQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByCodeAsync(query.Code, cancellationToken);
        return template is null || template.IsDeleted
            ? Result.Failure<ReportTemplateDto>(ReportsErrors.TemplateNotFound)
            : Result.Success(GetReportTemplateByIdQueryHandler.Map(template));
    }
}

public sealed record GetReportTemplatePreviewQuery(Guid Id) : IQuery<Result<GeneratedReportDto>>;

public sealed class GetReportTemplatePreviewQueryHandler(IReportTemplateRepository templates)
    : IQueryHandler<GetReportTemplatePreviewQuery, Result<GeneratedReportDto>>
{
    public async Task<Result<GeneratedReportDto>> HandleAsync(
        GetReportTemplatePreviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(query.Id, cancellationToken);
        if (template is null || template.IsDeleted)
            return Result.Failure<GeneratedReportDto>(ReportsErrors.TemplateNotFound);

        return Result.Success(new GeneratedReportDto(
            $"{SanitizeFileName(template.Name)}-preview.pdf",
            "application/pdf",
            template.PreviewPdf));
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "template" : sanitized.Trim();
    }
}


public sealed record RenderReportTemplatePreviewCommand(
    string HtmlContent,
    string SampleDataJson,
    string PageSize,
    string Orientation) : ICommand<Result<GeneratedReportDto>>;

public sealed class RenderReportTemplatePreviewCommandHandler(IReportDocumentGenerator generator)
    : ICommandHandler<RenderReportTemplatePreviewCommand, Result<GeneratedReportDto>>
{
    public async Task<Result<GeneratedReportDto>> HandleAsync(
        RenderReportTemplatePreviewCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!TemplateJsonValidator.IsValid(command.HtmlContent, command.SampleDataJson))
            return Result.Failure<GeneratedReportDto>(ReportsErrors.InvalidTemplate);

        try
        {
            var report = await generator.GenerateAsync(
                "pdf",
                command.HtmlContent,
                command.SampleDataJson,
                "template-preview",
                command.PageSize,
                command.Orientation,
                null,
                cancellationToken);
            return Result.Success(report);
        }
        catch
        {
            return Result.Failure<GeneratedReportDto>(ReportsErrors.GenerationFailed);
        }
    }
}

public sealed record CreateReportTemplateCommand(
    string? Code,
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
    string DataSchemaJson,
    string SampleDataJson,
    string PageSize,
    string Orientation,
    Guid? CreatedBy) : ICommand<Result<Guid>>;

public sealed class CreateReportTemplateCommandHandler(
    IReportTemplateRepository templates,
    IReportDocumentGenerator generator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateReportTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateReportTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!TemplateJsonValidator.IsValid(command.HtmlContent, command.DesignerJson, command.DataSchemaJson, command.SampleDataJson))
            return Result.Failure<Guid>(ReportsErrors.InvalidTemplate);

        if (await templates.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(ReportsErrors.TemplateNameAlreadyExists);

        var code = ReportTemplate.NormalizeTemplateCode(command.Code, command.Name);
        if (await templates.ExistsByCodeAsync(code, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(ReportsErrors.TemplateCodeAlreadyExists);

        byte[] preview;
        try
        {
            preview = (await generator.GenerateAsync(
                "pdf",
                command.HtmlContent,
                command.SampleDataJson,
                $"{code}-preview",
                command.PageSize,
                command.Orientation,
                null,
                cancellationToken)).Content;
        }
        catch
        {
            return Result.Failure<Guid>(ReportsErrors.GenerationFailed);
        }

        var template = ReportTemplate.Create(
            code,
            command.Name,
            command.Description,
            command.HtmlContent,
            command.DesignerJson,
            command.DataSchemaJson,
            command.SampleDataJson,
            command.PageSize,
            command.Orientation,
            preview,
            command.CreatedBy);

        await templates.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(template.Id);
    }
}

public sealed record UpdateReportTemplateCommand(
    Guid Id,
    string? Code,
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
    string DataSchemaJson,
    string SampleDataJson,
    string PageSize,
    string Orientation,
    Guid? UpdatedBy) : ICommand<Result>;

public sealed class UpdateReportTemplateCommandHandler(
    IReportTemplateRepository templates,
    IReportDocumentGenerator generator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateReportTemplateCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateReportTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(command.Id, cancellationToken);
        if (template is null || template.IsDeleted)
            return Result.Failure(ReportsErrors.TemplateNotFound);

        if (!TemplateJsonValidator.IsValid(command.HtmlContent, command.DesignerJson, command.DataSchemaJson, command.SampleDataJson))
            return Result.Failure(ReportsErrors.InvalidTemplate);

        if (await templates.ExistsByNameAsync(command.Name, command.Id, cancellationToken))
            return Result.Failure(ReportsErrors.TemplateNameAlreadyExists);

        var code = ReportTemplate.NormalizeTemplateCode(command.Code ?? template.Code, command.Name);
        if (await templates.ExistsByCodeAsync(code, command.Id, cancellationToken))
            return Result.Failure(ReportsErrors.TemplateCodeAlreadyExists);

        byte[] preview;
        try
        {
            preview = (await generator.GenerateAsync(
                "pdf",
                command.HtmlContent,
                command.SampleDataJson,
                $"{code}-preview",
                command.PageSize,
                command.Orientation,
                null,
                cancellationToken)).Content;
        }
        catch
        {
            return Result.Failure(ReportsErrors.GenerationFailed);
        }

        template.Update(
            code,
            command.Name,
            command.Description,
            command.HtmlContent,
            command.DesignerJson,
            command.DataSchemaJson,
            command.SampleDataJson,
            command.PageSize,
            command.Orientation,
            preview,
            command.UpdatedBy);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DeleteReportTemplateCommand(Guid Id, Guid? DeletedBy) : ICommand<Result>;

public sealed class DeleteReportTemplateCommandHandler(
    IReportTemplateRepository templates,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteReportTemplateCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteReportTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(command.Id, cancellationToken);
        if (template is null || template.IsDeleted)
            return Result.Failure(ReportsErrors.TemplateNotFound);

        template.Delete(command.DeletedBy);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record GenerateReportCommand(
    Guid TemplateId,
    string Format,
    string DataJson,
    string? FileName,
    string? SheetName) : ICommand<Result<GeneratedReportDto>>;

public sealed record GenerateReportByCodeCommand(
    string TemplateCode,
    string Format,
    string DataJson,
    string? FileName,
    string? SheetName) : ICommand<Result<GeneratedReportDto>>;

public sealed class GenerateReportCommandHandler(
    IReportTemplateRepository templates,
    IReportDocumentGenerator generator)
    : ICommandHandler<GenerateReportCommand, Result<GeneratedReportDto>>
{
    public async Task<Result<GeneratedReportDto>> HandleAsync(
        GenerateReportCommand command,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(command.TemplateId, cancellationToken);
        return await ReportGenerationExecutor.GenerateAsync(
            template, command.Format, command.DataJson, command.FileName, command.SheetName, generator, cancellationToken);
    }
}

public sealed class GenerateReportByCodeCommandHandler(
    IReportTemplateRepository templates,
    IReportDocumentGenerator generator)
    : ICommandHandler<GenerateReportByCodeCommand, Result<GeneratedReportDto>>
{
    public async Task<Result<GeneratedReportDto>> HandleAsync(
        GenerateReportByCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByCodeAsync(command.TemplateCode, cancellationToken);
        return await ReportGenerationExecutor.GenerateAsync(
            template, command.Format, command.DataJson, command.FileName, command.SheetName, generator, cancellationToken);
    }
}

internal static class ReportGenerationExecutor
{
    public static async Task<Result<GeneratedReportDto>> GenerateAsync(
        ReportTemplate? template,
        string format,
        string dataJson,
        string? fileName,
        string? sheetName,
        IReportDocumentGenerator generator,
        CancellationToken cancellationToken)
    {
        if (template is null || template.IsDeleted || !template.IsActive)
            return Result.Failure<GeneratedReportDto>(ReportsErrors.TemplateNotFound);

        var normalizedFormat = format.Trim().ToLowerInvariant();
        if (normalizedFormat is not ("pdf" or "xlsx" or "csv"))
            return Result.Failure<GeneratedReportDto>(ReportsErrors.UnsupportedFormat);

        if (!TemplateJsonValidator.IsJson(dataJson))
            return Result.Failure<GeneratedReportDto>(ReportsErrors.InvalidReportData);

        try
        {
            var resolvedFileName = string.IsNullOrWhiteSpace(fileName) ? template.Name : fileName.Trim();
            var report = await generator.GenerateAsync(
                normalizedFormat,
                template.HtmlContent,
                dataJson,
                resolvedFileName,
                template.PageSize,
                template.Orientation,
                sheetName,
                cancellationToken);
            return Result.Success(report);
        }
        catch
        {
            return Result.Failure<GeneratedReportDto>(ReportsErrors.GenerationFailed);
        }
    }
}

internal static class TemplateJsonValidator
{
    public static bool IsValid(string html, params string[] jsonValues) =>
        !string.IsNullOrWhiteSpace(html) && jsonValues.All(IsJson);

    public static bool IsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { using var _ = JsonDocument.Parse(value); return true; }
        catch (JsonException) { return false; }
    }
}
