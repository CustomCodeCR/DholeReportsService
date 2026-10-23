using System.Text.Json;

namespace Dhole.Reports.Infrastructure.Generation;

internal sealed record TabularData(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<IReadOnlyList<bool>>? NumericCells = null);

internal static class TabularDataReader
{
    public static TabularData Read(string dataJson)
    {
        using var document = JsonDocument.Parse(dataJson);
        var root = document.RootElement;
        var array = FindArray(root);

        if (array.ValueKind != JsonValueKind.Array)
            return new TabularData(["Value"], [[ToCell(root)]]);

        var objects = array.EnumerateArray().ToList();
        if (objects.Count == 0)
        {
            // A typed report can specify its column headers even when no rows match.
            if (root.ValueKind == JsonValueKind.Object &&
                TryGetPropertyIgnoreCase(root, "columns", out var columns) &&
                columns.ValueKind == JsonValueKind.Array)
                return new TabularData(columns.EnumerateArray()
                    .Select(column => column.GetString() ?? string.Empty).ToList(), []);

            return new TabularData([], []);
        }

        if (objects.All(x => x.ValueKind == JsonValueKind.Object))
        {
            var headers = objects
                .SelectMany(x => x.EnumerateObject().Select(p => p.Name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var rows = objects.Select(item =>
                (IReadOnlyList<string>)headers.Select(header =>
                {
                    foreach (var property in item.EnumerateObject())
                        if (property.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                            return ToCell(property.Value);
                    return string.Empty;
                }).ToList()).ToList();

            // Preserve JSON numeric types for Excel: money is numeric, but QUO/IDTRA
            // (including values with leading zeros) remain strings.
            var numericCells = objects.Select(item =>
                (IReadOnlyList<bool>)headers.Select(header =>
                    item.EnumerateObject().Any(property =>
                        property.Name.Equals(header, StringComparison.OrdinalIgnoreCase) &&
                        property.Value.ValueKind == JsonValueKind.Number)).ToList()).ToList();

            return new TabularData(headers, rows, numericCells);
        }

        return new TabularData(["Value"], objects.Select(x => (IReadOnlyList<string>)[ToCell(x)]).ToList());
    }

    private static JsonElement FindArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array) return root;
        if (root.ValueKind != JsonValueKind.Object) return default;

        foreach (var preferred in new[] { "rows", "items", "data", "results" })
            if (TryGetPropertyIgnoreCase(root, preferred, out var value) && value.ValueKind == JsonValueKind.Array)
                return value;

        foreach (var property in root.EnumerateObject())
            if (property.Value.ValueKind == JsonValueKind.Array)
                return property.Value;

        return default;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string ToCell(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        _ => value.GetRawText()
    };
}
