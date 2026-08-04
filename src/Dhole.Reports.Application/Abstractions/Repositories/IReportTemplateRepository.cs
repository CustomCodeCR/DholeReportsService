using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Reports.Contracts.Templates;
using Dhole.Reports.Domain.Templates.Entities;

namespace Dhole.Reports.Application.Abstractions.Repositories;

public interface IReportTemplateRepository : IRepository<ReportTemplate, Guid>
{
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ReportTemplateListDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
