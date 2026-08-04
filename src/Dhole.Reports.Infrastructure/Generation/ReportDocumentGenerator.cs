using System.Diagnostics;
using System.Text;
using Dhole.Reports.Application.Abstractions.Generation;
using Dhole.Reports.Contracts.Generation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dhole.Reports.Infrastructure.Generation;

public sealed class ReportDocumentGenerator(
    IOptions<ReportGenerationOptions> options,
    ILogger<ReportDocumentGenerator> logger) : IReportDocumentGenerator
{
    private readonly ReportGenerationOptions _options = options.Value;

    public async Task<byte[]> RenderPdfAsync(
        string html,
        string pageSize,
        string orientation,
        CancellationToken cancellationToken = default)
    {
        var documentHtml = EnsureDocument(html, pageSize, orientation);
        var chromium = ResolveExecutable(
            _options.ChromiumExecutablePath,
            "chromium",
            "chromium-browser",
            "google-chrome",
            "google-chrome-stable");

        if (chromium is not null)
        {
            try
            {
                return await RenderWithChromiumAsync(chromium, documentHtml, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Chromium no pudo renderizar el PDF. Se intentará WeasyPrint.");
            }
        }

        var weasyPrint = ResolveExecutable(
            _options.WeasyPrintExecutablePath,
            "weasyprint");

        if (weasyPrint is not null)
        {
            try
            {
                return await RenderWithWeasyPrintAsync(weasyPrint, documentHtml, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "WeasyPrint no pudo renderizar el PDF.");
            }
        }

        if (_options.AllowBasicPdfFallback)
        {
            logger.LogWarning(
                "No se pudo usar Chromium ni WeasyPrint. Se usará el generador PDF básico.");
            return BasicPdfWriter.Write(documentHtml);
        }

        throw new InvalidOperationException(
            "No fue posible renderizar HTML a PDF. Configure Chromium o WeasyPrint.");
    }

    public async Task<GeneratedReportDto> GenerateAsync(
        string format,
        string html,
        string dataJson,
        string fileName,
        string pageSize,
        string orientation,
        string? sheetName,
        CancellationToken cancellationToken = default)
    {
        var safeName = SanitizeFileName(fileName);
        return format switch
        {
            "pdf" => new GeneratedReportDto(
                EnsureExtension(safeName, ".pdf"),
                "application/pdf",
                await RenderPdfAsync(
                    TemplateDataBinder.Bind(html, dataJson),
                    pageSize,
                    orientation,
                    cancellationToken)),
            "xlsx" => new GeneratedReportDto(
                EnsureExtension(safeName, ".xlsx"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                XlsxReportWriter.Write(TabularDataReader.Read(dataJson), sheetName)),
            "csv" => new GeneratedReportDto(
                EnsureExtension(safeName, ".csv"),
                "text/csv; charset=utf-8",
                CsvReportWriter.Write(TabularDataReader.Read(dataJson))),
            _ => throw new NotSupportedException($"Formato no soportado: {format}")
        };
    }

    private async Task<byte[]> RenderWithChromiumAsync(
        string executable,
        string html,
        CancellationToken cancellationToken)
    {
        var tempDirectory = CreateTempDirectory();
        var htmlPath = Path.Combine(tempDirectory, "report.html");
        var pdfPath = Path.Combine(tempDirectory, "report.pdf");

        try
        {
            await File.WriteAllTextAsync(htmlPath, html, Encoding.UTF8, cancellationToken);
            var fileUri = new Uri(htmlPath).AbsoluteUri;
            var arguments = new[]
            {
                "--headless=new",
                "--no-sandbox",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-extensions",
                "--disable-javascript",
                "--disable-background-networking",
                "--no-first-run",
                "--run-all-compositor-stages-before-draw",
                "--virtual-time-budget=3000",
                $"--user-data-dir={Path.Combine(tempDirectory, "chromium-profile")}",
                $"--print-to-pdf={pdfPath}",
                fileUri
            };

            var result = await RunProcessAsync(
                executable,
                arguments,
                TimeSpan.FromSeconds(Math.Max(10, _options.ChromiumTimeoutSeconds)),
                cancellationToken);

            if (result.ExitCode != 0 || !File.Exists(pdfPath))
            {
                throw new InvalidOperationException(
                    $"Chromium no generó el PDF. {result.StandardError}".Trim());
            }

            return await File.ReadAllBytesAsync(pdfPath, cancellationToken);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    private async Task<byte[]> RenderWithWeasyPrintAsync(
        string executable,
        string html,
        CancellationToken cancellationToken)
    {
        var tempDirectory = CreateTempDirectory();
        var htmlPath = Path.Combine(tempDirectory, "report.html");
        var pdfPath = Path.Combine(tempDirectory, "report.pdf");

        try
        {
            await File.WriteAllTextAsync(htmlPath, html, Encoding.UTF8, cancellationToken);
            var result = await RunProcessAsync(
                executable,
                [htmlPath, pdfPath],
                TimeSpan.FromSeconds(Math.Max(10, _options.WeasyPrintTimeoutSeconds)),
                cancellationToken);

            if (result.ExitCode != 0 || !File.Exists(pdfPath))
            {
                throw new InvalidOperationException(
                    $"WeasyPrint no generó el PDF. {result.StandardError}".Trim());
            }

            return await File.ReadAllBytesAsync(pdfPath, cancellationToken);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    private static async Task<ProcessResult> RunProcessAsync(
        string executable,
        IReadOnlyCollection<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"No fue posible iniciar {executable}.");
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        var standardOutput = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var standardError = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
            return new ProcessResult(
                process.ExitCode,
                await standardOutput,
                await standardError);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException(
                $"El proceso {Path.GetFileName(executable)} superó {timeout.TotalSeconds:0} segundos.");
        }
        catch
        {
            TryKill(process);
            throw;
        }
    }

    private static string? ResolveExecutable(string? configuredPath, params string[] candidates)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
            return configuredPath;

        foreach (var candidate in candidates)
        {
            var resolved = ResolveFromPath(candidate);
            if (resolved is not null) return resolved;
        }

        return null;
    }

    private static string? ResolveFromPath(string executable)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory, executable);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static string EnsureDocument(string html, string pageSize, string orientation)
    {
        if (html.Contains("<html", StringComparison.OrdinalIgnoreCase)) return html;
        var size = string.IsNullOrWhiteSpace(pageSize) ? "A4" : pageSize.ToUpperInvariant();
        var landscape = orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase)
            ? " landscape"
            : string.Empty;

        return $$"""
            <!doctype html><html><head><meta charset="utf-8">
            <style>@page { size: {{size}}{{landscape}}; margin: 12mm; } body { margin: 0; font-family: Arial, sans-serif; }</style>
            </head><body>{{html}}</body></html>
            """;
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var result = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(result) ? "reporte" : result;
    }

    private static string EnsureExtension(string fileName, string extension) =>
        fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? fileName
            : fileName + extension;

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "dhole-reports", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        try { Directory.Delete(path, recursive: true); }
        catch { }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch { }
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
