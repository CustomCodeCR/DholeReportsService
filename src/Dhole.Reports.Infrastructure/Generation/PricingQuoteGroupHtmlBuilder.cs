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
.quote-group-index { page-break-after:always; break-after:page; font-family:Arial,Helvetica,sans-serif; padding:3mm 1mm 5mm; color:#252525; }
.quote-group-index * { box-sizing:border-box; }
.quote-group-hero { background:#a80e18; color:#fff; padding:21px 24px 22px; border-radius:12px; }
.quote-group-hero .eyebrow { font-size:9px; font-weight:800; letter-spacing:2px; text-transform:uppercase; color:#ffe5e7; margin:0 0 8px; }
.quote-group-hero h1 { color:#fff; font-size:25px; line-height:1.2; margin:0 0 7px; font-weight:800; }
.quote-group-hero .quote-number { color:#fff; font-size:12px; font-weight:700; margin:0; letter-spacing:.5px; }
.quote-group-hero .intro { color:#fff0f0; font-size:10px; line-height:1.5; margin:10px 0 0; }
.quote-group-heading { display:flex; justify-content:space-between; align-items:center; margin:18px 0 10px; border-bottom:2px solid #a80e18; padding-bottom:7px; }
.quote-group-heading strong { font-size:12px; color:#252525; letter-spacing:.8px; text-transform:uppercase; }
.quote-group-count { border-radius:14px; color:#a80e18; background:#fbecee; font-weight:800; font-size:10px; padding:5px 9px; }
.quote-group-card { display:block; border:1px solid #e6dedf; border-left:5px solid #a80e18; border-radius:10px; margin-bottom:11px; padding:13px 16px; color:#252525; text-decoration:none; background:#fff; break-inside:avoid; page-break-inside:avoid; }
.quote-group-card:hover { background:#fff8f8; }
.quote-group-card-head { display:flex; align-items:center; justify-content:space-between; gap:8px; margin-bottom:8px; }
.quote-group-card-index { display:inline-block; background:#a80e18; color:#fff; border-radius:7px; padding:5px 8px; font-size:10px; font-weight:800; }
.quote-group-card-number { font-weight:800; color:#a80e18; font-size:13px; margin-left:8px; }
.quote-group-card-link { color:#a80e18; font-size:10px; font-weight:700; text-decoration:underline; }
.quote-group-card-route { color:#2d3032; font-weight:700; font-size:11px; line-height:1.4; margin:0 0 11px; }
.quote-group-card-meta { display:table; width:100%; padding-top:9px; border-top:1px solid #e9e3e4; }
.quote-group-card-meta-item { display:table-cell; width:50%; vertical-align:top; }
.quote-group-card-meta-label { display:block; color:#777; font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:.8px; margin-bottom:3px; }
.quote-group-card-meta-value { display:block; color:#292929; font-size:12px; font-weight:800; overflow-wrap:anywhere; }
.quote-group-card-of { color:#a80e18; font-size:14px; }
.quote-group-option + .quote-group-option { break-before:page; page-break-before:always; }
.quote-group-back { margin-top:15px; font-size:9px; }
.quote-group-back a { color:#a80e18; }
</style></head><body>
<section class="quote-group-index" id="quo-index">
""");
        output.Append("<div class=\"quote-group-hero\"><p class=\"eyebrow\">Grupo Castro Fallas · Cotización marítima</p>");
        output.Append("<h1>Opciones de transporte</h1><p class=\"quote-number\">")
            .Append(Encode(parentQuoteNumber)).Append("</p>");
        output.Append("<p class=\"intro\">Compare rutas, navieras y Ocean Freight. Cada alternativa abre su cotización comercial completa dentro de este documento.</p></div>");
        output.Append("<div class=\"quote-group-heading\"><strong>Índice de subcotizaciones</strong><span class=\"quote-group-count\">")
            .Append(prepared.Count).Append(" opciones</span></div>");
        for (var index = 0; index < prepared.Count; index++)
        {
            var (item, suffix, _) = prepared[index];
            output.Append("<a class=\"quote-group-card\" href=\"#quo-option-").Append(suffix).Append("\">")
                .Append("<div class=\"quote-group-card-head\"><div><span class=\"quote-group-card-index\">")
                .Append((index + 1).ToString("00")).Append("</span><span class=\"quote-group-card-number\">")
                .Append(Encode(item.QuoteNumber)).Append("</span></div><span class=\"quote-group-card-link\">Ver detalle →</span></div>")
                .Append("<p class=\"quote-group-card-route\">").Append(Encode(item.Route)).Append("</p>")
                .Append("<div class=\"quote-group-card-meta\"><div class=\"quote-group-card-meta-item\"><span class=\"quote-group-card-meta-label\">Naviera</span>")
                .Append("<strong class=\"quote-group-card-meta-value\">").Append(Encode(item.Carrier)).Append("</strong></div>")
                .Append("<div class=\"quote-group-card-meta-item\"><span class=\"quote-group-card-meta-label\">Ocean Freight (OF)</span>")
                .Append("<strong class=\"quote-group-card-meta-value quote-group-card-of\">")
                .Append(Encode(item.Currency)).Append(' ').Append(Encode(item.OfAmount))
                .Append("</strong></div></div></a>");
        }
        output.Append("</section>");

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
