namespace Dhole.Reports.Contracts.Generation;

public sealed record GenerateTabularReportRequest(
    string Format,
    string DataJson,
    string? FileName = null,
    string? SheetName = null
);
