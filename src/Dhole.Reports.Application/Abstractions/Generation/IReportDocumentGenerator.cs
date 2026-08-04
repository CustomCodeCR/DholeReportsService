using Dhole.Reports.Contracts.Generation;

namespace Dhole.Reports.Application.Abstractions.Generation;

public interface IReportDocumentGenerator
{
    Task<byte[]> RenderPdfAsync(
        string html,
        string pageSize,
        string orientation,
        CancellationToken cancellationToken = default);

    Task<GeneratedReportDto> GenerateAsync(
        string format,
        string html,
        string dataJson,
        string fileName,
        string pageSize,
        string orientation,
        string? sheetName,
        CancellationToken cancellationToken = default);
}
