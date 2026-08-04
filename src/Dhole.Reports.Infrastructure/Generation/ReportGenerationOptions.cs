namespace Dhole.Reports.Infrastructure.Generation;

public sealed class ReportGenerationOptions
{
    public string? ChromiumExecutablePath { get; init; }
    public int ChromiumTimeoutSeconds { get; init; } = 60;
    public string? WeasyPrintExecutablePath { get; init; }
    public int WeasyPrintTimeoutSeconds { get; init; } = 60;
    public bool AllowBasicPdfFallback { get; init; } = true;
}
