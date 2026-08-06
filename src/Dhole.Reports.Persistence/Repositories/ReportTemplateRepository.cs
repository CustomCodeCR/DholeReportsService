using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Reports.Application.Abstractions.Repositories;
using Dhole.Reports.Contracts.Templates;
using Dhole.Reports.Domain.Templates.Entities;
using Dhole.Reports.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Reports.Persistence.Repositories;

public sealed class ReportTemplateRepository(ServiceDbContext dbContext)
    : EfRepository<ReportTemplate, Guid>(dbContext), IReportTemplateRepository
{
    public Task<ReportTemplate?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var value = code.Trim().ToLowerInvariant();
        return dbContext.ReportTemplates.FirstOrDefaultAsync(
            x => x.Code.ToLower() == value && !x.IsDeleted,
            cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var value = name.Trim().ToLowerInvariant();
        return dbContext.ReportTemplates.IgnoreQueryFilters().AnyAsync(
            x => x.Name.ToLower() == value
                && !x.IsDeleted
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var value = code.Trim().ToLowerInvariant();
        return dbContext.ReportTemplates.IgnoreQueryFilters().AnyAsync(
            x => x.Code.ToLower() == value
                && !x.IsDeleted
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<PagedResult<ReportTemplateListDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReportTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || (x.Description != null && x.Description.ToLower().Contains(value)));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new ReportTemplateListDto(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.PageSize,
                x.Orientation,
                x.PreviewPdf.Length > 0,
                x.PreviewGeneratedAtUtc,
                x.IsActive,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<ReportTemplateListDto>.Create(
            items, page.PageNumber, page.PageSize, total);
    }
}
