using System.Text;

namespace Dhole.Reports.Infrastructure.Generation;

internal static class CsvReportWriter
{
    public static byte[] Write(TabularData table)
    {
        var builder = new StringBuilder();
        if (table.Headers.Count > 0)
            builder.AppendLine(string.Join(',', table.Headers.Select(Escape)));

        foreach (var row in table.Rows)
            builder.AppendLine(string.Join(',', row.Select(Escape)));

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(builder.ToString());
    }

    private static string Escape(string value)
    {
        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
        return normalized.IndexOfAny([',', '"', '\n']) >= 0
            ? $"\"{normalized.Replace("\"", "\"\"")}\""
            : normalized;
    }
}
