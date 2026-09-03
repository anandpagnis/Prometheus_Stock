using System.ComponentModel.DataAnnotations;

namespace PrometheusStock.Api.MarketData.Yahoo;

/// <summary>
/// Binds the <c>YahooFinance</c> configuration section. Validated at host start-up
/// (<c>ValidateOnStart</c>) so a missing User-Agent or a malformed base URL fails
/// the process immediately rather than on the first request.
/// </summary>
public sealed class YahooFinanceOptions
{
    /// <summary>Configuration section this class binds to.</summary>
    public const string SectionName = "YahooFinance";

    /// <summary>Scheme and host of the Yahoo chart API, e.g. <c>https://query1.finance.yahoo.com/</c>.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// User-Agent sent on every request. Yahoo rejects the default .NET agent, so a
    /// browser-like value is mandatory.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string UserAgent { get; init; } = string.Empty;

    /// <summary>Per-request timeout for a single Yahoo call.</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Look-back window passed as the <c>range</c> query parameter. Fixed at <c>1mo</c> for the MVP.</summary>
    [Required]
    public string Range { get; init; } = "1mo";

    /// <summary>Bar granularity passed as the <c>interval</c> query parameter.</summary>
    [Required]
    public string Interval { get; init; } = "15m";
}
