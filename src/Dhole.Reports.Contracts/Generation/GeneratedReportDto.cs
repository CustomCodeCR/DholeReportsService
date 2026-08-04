namespace Dhole.Reports.Contracts.Generation;

public sealed record GeneratedReportDto(
    string FileName,
    string ContentType,
    byte[] Content);
