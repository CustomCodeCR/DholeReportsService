namespace Dhole.Reports.Contracts.Generation;

/// <summary>
/// Pricing builds every DataJson from persisted commercial snapshots.
/// The OF amount is the sale of international freight, never the internal cost.
/// </summary>
public sealed record GenerateGroupedPricingQuoteRequest(
    string ParentQuoteNumber,
    IReadOnlyList<GroupedPricingQuoteAlternativeRequest> Alternatives);

public sealed record GroupedPricingQuoteAlternativeRequest(
    string QuoteNumber,
    string Route,
    string Carrier,
    string OfAmount,
    string Currency,
    string DataJson,
    string TemplateCode = "pricing-fcl-client-quote");
