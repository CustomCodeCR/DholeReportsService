using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Dhole.Reports.Infrastructure.Generation;

public sealed record GroupedPricingQuoteRenderItem(
    string QuoteNumber,
    string Route,
    string Carrier,
    string OfAmount,
    string Currency,
    string HtmlTemplate,
    string DataJson);

/// <summary>
/// Produces one HTML document to print as one PDF, preserving each original
/// commercial template body. Chromium and WeasyPrint resolve internal #links.
/// </summary>
public static partial class PricingQuoteGroupHtmlBuilder
{
    [GeneratedRegex(@"<head\b[^>]*>([\s\S]*?)</head>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadRegex();

    [GeneratedRegex(@"<body\b[^>]*>([\s\S]*?)</body>", RegexOptions.IgnoreCase)]
    private static partial Regex BodyRegex();

    [GeneratedRegex(@"^QUO-[0-9]{5}-[0-9]{6}-([A-Z]+)$", RegexOptions.CultureInvariant)]
    private static partial Regex AlternativeNumberRegex();

    public static string Build(string parentQuoteNumber, IReadOnlyList<GroupedPricingQuoteRenderItem> alternatives)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentQuoteNumber);
        ArgumentNullException.ThrowIfNull(alternatives);
        if (!Regex.IsMatch(parentQuoteNumber, @"^QUO-[0-9]{5}-[0-9]{6}$"))
            throw new ArgumentException("Invalid parent QUO.", nameof(parentQuoteNumber));
        if (alternatives.Count is < 2 or > 100)
            throw new ArgumentException("A group requires 2-100 alternatives.", nameof(alternatives));

        var firstHead = HeadRegex().Match(alternatives[0].HtmlTemplate);
        if (!firstHead.Success)
            throw new ArgumentException("Commercial template has no HTML head.");

        var prepared = new List<(GroupedPricingQuoteRenderItem Option, string Suffix, string Body)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in alternatives)
        {
            var match = AlternativeNumberRegex().Match(item.QuoteNumber);
            if (!match.Success || !item.QuoteNumber.StartsWith(parentQuoteNumber + "-", StringComparison.Ordinal))
                throw new ArgumentException("An alternative belongs to another QUO group.");
            if (!seen.Add(item.QuoteNumber))
                throw new ArgumentException("Duplicate sub-QUO identifier.");
            if (string.IsNullOrWhiteSpace(item.HtmlTemplate) || string.IsNullOrWhiteSpace(item.DataJson))
                throw new ArgumentException("Each alternative requires template and commercial data.");

            // Binding individually is essential: totals and commercial terms must
            // never leak from the currently selected route into a different route.
            var html = TemplateDataBinder.Bind(item.HtmlTemplate, item.DataJson);
            var body = BodyRegex().Match(html);
            if (!body.Success)
                throw new ArgumentException("Commercial template has no HTML body.");
            prepared.Add((item, match.Groups[1].Value, body.Groups[1].Value));
        }

        static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        var output = new StringBuilder("<!DOCTYPE html><html lang=\"es\"><head>");
        output.Append(firstHead.Groups[1].Value);
        output.Append("""
<style>
.quote-group-index { page-break-after:always; break-after:page; font-family:Arial,Helvetica,sans-serif; padding:6mm 2mm; }
.quote-group-index h1 { color:#ad1220; font-size:23px; margin:10px 0; }
.quote-group-index .intro { color:#555; font-size:11px; margin-bottom:20px; }
.quote-group-table { border-collapse:collapse; width:100%; table-layout:fixed; }
.quote-group-table th { background:#252525; color:#fff; text-align:left; padding:10px 7px; font-size:10px; }
.quote-group-table td { border-bottom:1px solid #ddd; padding:10px 7px; overflow-wrap:anywhere; font-size:10px; }
.quote-group-table tr { break-inside:avoid; page-break-inside:avoid; }
.quote-group-table a { color:#ad1220; font-weight:800; text-decoration:underline; }
.quote-group-option + .quote-group-option { break-before:page; page-break-before:always; }
.quote-group-back { margin-top:15px; font-size:9px; }
.quote-group-back a { color:#ad1220; }
</style></head><body>
<section class="quote-group-index" id="quo-index">
""");
        output.Append("<h1>Cotización ").Append(Encode(parentQuoteNumber)).Append("</h1>");
        output.Append("<p class=\"intro\">Opciones comerciales de ruta, naviera y flete oceánico (OF). Seleccione una opción para consultar su cotización completa.</p>");
        output.Append("<table class=\"quote-group-table\"><thead><tr><th style=\"width:25%\">Sub-QUO</th><th style=\"width:31%\">Ruta</th><th style=\"width:23%\">Naviera</th><th style=\"width:21%\">OF</th></tr></thead><tbody>");
        foreach (var (item, suffix, _) in prepared)
        {
            output.Append("<tr><td><a href=\"#quo-option-").Append(suffix).Append("\">")
                .Append(Encode(item.QuoteNumber)).Append("</a></td><td>")
                .Append(Encode(item.Route)).Append("</td><td>")
                .Append(Encode(item.Carrier)).Append("</td><td>")
                .Append(Encode(item.Currency)).Append(' ')
                .Append(Encode(item.OfAmount)).Append("</td></tr>");
        }
        output.Append("</tbody></table></section>");

        foreach (var (_, suffix, body) in prepared)
        {
            output.Append("<article id=\"quo-option-").Append(suffix)
                .Append("\" class=\"quote-group-option\">")
                .Append(body)
                .Append("<p class=\"quote-group-back\"><a href=\"#quo-index\">Volver al índice de alternativas</a></p></article>");
        }

        output.Append("</body></html>");
        return output.ToString();
    }
}
