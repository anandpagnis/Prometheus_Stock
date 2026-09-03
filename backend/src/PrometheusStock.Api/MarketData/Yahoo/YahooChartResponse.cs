using System.Text.Json.Serialization;

namespace PrometheusStock.Api.MarketData.Yahoo;

// Wire shape of Yahoo Finance's v8/finance/chart response. Deliberately internal:
// nothing outside this folder should know Yahoo's JSON, only the mapped IntradayBar.

internal sealed class YahooChartResponse
{
    [JsonPropertyName("chart")]
    public YahooChart? Chart { get; init; }
}

internal sealed class YahooChart
{
    [JsonPropertyName("result")]
    public IReadOnlyList<YahooChartResult>? Result { get; init; }

    [JsonPropertyName("error")]
    public YahooChartError? Error { get; init; }
}

internal sealed class YahooChartError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

internal sealed class YahooChartResult
{
    [JsonPropertyName("meta")]
    public YahooChartMeta? Meta { get; init; }

    /// <summary>Unix seconds (UTC), one per bar, ascending.</summary>
    [JsonPropertyName("timestamp")]
    public IReadOnlyList<long>? Timestamp { get; init; }

    [JsonPropertyName("indicators")]
    public YahooChartIndicators? Indicators { get; init; }
}

internal sealed class YahooChartMeta
{
    /// <summary>IANA id of the exchange time zone, e.g. <c>America/New_York</c>. Preferred over <see cref="Gmtoffset" />.</summary>
    [JsonPropertyName("exchangeTimezoneName")]
    public string? ExchangeTimezoneName { get; init; }

    /// <summary>Scalar offset in seconds. Only a single value for the whole payload, so wrong across a DST change — used only when <see cref="ExchangeTimezoneName" /> is null.</summary>
    [JsonPropertyName("gmtoffset")]
    public int? Gmtoffset { get; init; }
}

internal sealed class YahooChartIndicators
{
    [JsonPropertyName("quote")]
    public IReadOnlyList<YahooChartQuote>? Quote { get; init; }
}

internal sealed class YahooChartQuote
{
    [JsonPropertyName("high")]
    public IReadOnlyList<decimal?>? High { get; init; }

    [JsonPropertyName("low")]
    public IReadOnlyList<decimal?>? Low { get; init; }

    [JsonPropertyName("volume")]
    public IReadOnlyList<long?>? Volume { get; init; }
}
