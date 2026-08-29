using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dhole.Reports.Infrastructure.Generation;

internal static partial class TemplateDataBinder
{
    public static string Bind(string html, string dataJson)
    {
        using var document = JsonDocument.Parse(dataJson);
        var root = document.RootElement;

        var withConditionals = IfRegex().Replace(html, match =>
        {
            var path = match.Groups[1].Value.Trim();
            var body = match.Groups[2].Value;
            return TryResolve(root, path, out var value) && IsTruthy(value)
                ? body
                : string.Empty;
        });

        var withLoops = EachRegex().Replace(withConditionals, match =>
        {
            var path = match.Groups[1].Value.Trim();
            var body = match.Groups[2].Value;
            if (!TryResolve(root, path, out var value) || value.ValueKind != JsonValueKind.Array)
                return string.Empty;

            return string.Concat(value.EnumerateArray().Select(item => ReplaceScalars(body, item, root)));
        });

        return ReplaceScalars(withLoops, root, root);
    }

    private static bool IsTruthy(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => !string.IsNullOrWhiteSpace(value.GetString()),
        JsonValueKind.Number => value.TryGetDecimal(out var number) && number != 0m,
        JsonValueKind.True => true,
        JsonValueKind.Array => value.GetArrayLength() > 0,
        JsonValueKind.Object => true,
        _ => false
    };

    private static string ReplaceScalars(string input, JsonElement context, JsonElement root)
    {
        return ScalarRegex().Replace(input, match =>
        {
            var path = match.Groups[1].Value.Trim();
            JsonElement value;

            if (path.StartsWith("root.", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolve(root, path[5..], out value)) return string.Empty;
            }
            else if (!TryResolve(context, path, out value) && !TryResolve(root, path, out value))
            {
                return string.Empty;
            }

            return WebUtility.HtmlEncode(ToDisplayString(value));
        });
    }

    private static bool TryResolve(JsonElement element, string path, out JsonElement value)
    {
        value = element;
        if (path is "." or "this") return true;

        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (value.ValueKind != JsonValueKind.Object || !TryGetPropertyIgnoreCase(value, segment, out value))
                return false;
        }

        return true;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value)) return true;
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

    private static string ToDisplayString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "Sí",
        JsonValueKind.False => "No",
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        _ => value.GetRawText()
    };

    [GeneratedRegex(@"\{\{#if\s+([^}]+)\}\}([\s\S]*?)\{\{/if\}\}", RegexOptions.IgnoreCase)]
    private static partial Regex IfRegex();

    [GeneratedRegex(@"\{\{#each\s+([^}]+)\}\}([\s\S]*?)\{\{/each\}\}", RegexOptions.IgnoreCase)]
    private static partial Regex EachRegex();

    [GeneratedRegex(@"\{\{\s*([^#/{][^}]*)\s*\}\}")]
    private static partial Regex ScalarRegex();
}
