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

public sealed record GetReportTemplateByIdQuery(Guid Id)
    : IQuery<Result<ReportTemplateDto>>;

public sealed class GetReportTemplateByIdQueryHandler(IReportTemplateRepository templates)
    : IQueryHandler<GetReportTemplateByIdQuery, Result<ReportTemplateDto>>
{
    public async Task<Result<ReportTemplateDto>> HandleAsync(
        GetReportTemplateByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(query.Id, cancellationToken);
        if (template is null || template.IsDeleted)
            return Result.Failure<ReportTemplateDto>(ReportsErrors.TemplateNotFound);

        return Result.Success(Map(template));
    }

    internal static ReportTemplateDto Map(ReportTemplate template) => new(
        template.Id,
        template.Name,
        template.Description,
        template.HtmlContent,
        template.DesignerJson,
        template.PageSize,
        template.Orientation,
        template.PreviewPdf.Length > 0,
        template.PreviewGeneratedAtUtc,
        template.IsActive,
        template.CreatedAtUtc,
        template.UpdatedAtUtc);
}

public sealed record GetReportTemplatePreviewQuery(Guid Id)
    : IQuery<Result<GeneratedReportDto>>;

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

public sealed record CreateReportTemplateCommand(
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
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
        if (!IsValid(command.HtmlContent, command.DesignerJson))
            return Result.Failure<Guid>(ReportsErrors.InvalidTemplate);

        if (await templates.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken))
            return Result.Failure<Guid>(ReportsErrors.TemplateNameAlreadyExists);

        byte[] preview;
        try
        {
            preview = await generator.RenderPdfAsync(
                command.HtmlContent,
                command.PageSize,
                command.Orientation,
                cancellationToken);
        }
        catch
        {
            return Result.Failure<Guid>(ReportsErrors.GenerationFailed);
        }

        var template = ReportTemplate.Create(
            command.Name,
            command.Description,
            command.HtmlContent,
            command.DesignerJson,
            command.PageSize,
            command.Orientation,
            preview,
            command.CreatedBy);

        await templates.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(template.Id);
    }

    private static bool IsValid(string html, string designerJson)
    {
        if (string.IsNullOrWhiteSpace(html) || string.IsNullOrWhiteSpace(designerJson)) return false;
        try { using var _ = JsonDocument.Parse(designerJson); return true; }
        catch (JsonException) { return false; }
    }
}

public sealed record UpdateReportTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    string HtmlContent,
    string DesignerJson,
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

        if (!IsValid(command.HtmlContent, command.DesignerJson))
            return Result.Failure(ReportsErrors.InvalidTemplate);

        if (await templates.ExistsByNameAsync(
                command.Name,
                command.Id,
                cancellationToken))
            return Result.Failure(ReportsErrors.TemplateNameAlreadyExists);

        byte[] preview;
        try
        {
            preview = await generator.RenderPdfAsync(
                command.HtmlContent,
                command.PageSize,
                command.Orientation,
                cancellationToken);
        }
        catch
        {
            return Result.Failure(ReportsErrors.GenerationFailed);
        }

        template.Update(
            command.Name,
            command.Description,
            command.HtmlContent,
            command.DesignerJson,
            command.PageSize,
            command.Orientation,
            preview,
            command.UpdatedBy);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static bool IsValid(string html, string designerJson)
    {
        if (string.IsNullOrWhiteSpace(html) || string.IsNullOrWhiteSpace(designerJson)) return false;
        try { using var _ = JsonDocument.Parse(designerJson); return true; }
        catch (JsonException) { return false; }
    }
}

public sealed record DeleteReportTemplateCommand(Guid Id, Guid? DeletedBy)
    : ICommand<Result>;

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
        if (template is null || template.IsDeleted || !template.IsActive)
            return Result.Failure<GeneratedReportDto>(ReportsErrors.TemplateNotFound);

        var format = command.Format.Trim().ToLowerInvariant();
        if (format is not ("pdf" or "xlsx" or "csv"))
            return Result.Failure<GeneratedReportDto>(ReportsErrors.UnsupportedFormat);

        try { using var _ = JsonDocument.Parse(command.DataJson); }
        catch (JsonException) { return Result.Failure<GeneratedReportDto>(ReportsErrors.InvalidReportData); }

        try
        {
            var fileName = string.IsNullOrWhiteSpace(command.FileName)
                ? template.Name
                : command.FileName.Trim();

            var report = await generator.GenerateAsync(
                format,
                template.HtmlContent,
                command.DataJson,
                fileName,
                template.PageSize,
                template.Orientation,
                command.SheetName,
                cancellationToken);

            return Result.Success(report);
        }
        catch
        {
            return Result.Failure<GeneratedReportDto>(ReportsErrors.GenerationFailed);
        }
    }
}
