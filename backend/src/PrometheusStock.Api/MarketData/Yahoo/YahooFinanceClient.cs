using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Options;

namespace PrometheusStock.Api.MarketData.Yahoo;

/// <summary>
/// <see cref="IStockDataProvider" /> backed by Yahoo Finance's
/// <c>v8/finance/chart</c> endpoint. Registered as a typed <see cref="HttpClient" />
/// in <c>AddMarketData</c>, where the base address and User-Agent are configured
/// from <see cref="YahooFinanceOptions" />.
/// </summary>
public sealed class YahooFinanceClient(HttpClient httpClient, IOptions<YahooFinanceOptions> options)
    : IStockDataProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient = httpClient;
    private readonly YahooFinanceOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        string requestUri =
            $"v8/finance/chart/{Uri.EscapeDataString(symbol)}?interval={_options.Interval}&range={_options.Range}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new UpstreamException($"Yahoo Finance could not be reached for '{symbol}'.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new SymbolNotFoundException(symbol);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new UpstreamException(
                    $"Yahoo Finance returned HTTP {(int)response.StatusCode} for '{symbol}'.");
            }

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseAndMap(symbol, body);
        }
    }

    private static IReadOnlyList<IntradayBar> ParseAndMap(string symbol, string body)
    {
        YahooChartResponse? payload;
        try
        {
            payload = JsonSerializer.Deserialize<YahooChartResponse>(body, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new UpstreamException($"Yahoo Finance sent an unparseable response for '{symbol}'.", ex);
        }

        if (payload?.Chart is null)
        {
            throw new UpstreamException($"Yahoo Finance sent an unrecognised response for '{symbol}'.");
        }

        YahooChartResult? result = payload.Chart.Result is { Count: > 0 } results ? results[0] : null;
        if (result is null)
        {
            // result:null (with an error node) is Yahoo's "no such symbol" shape.
            throw new SymbolNotFoundException(symbol);
        }

        try
        {
            return MapBars(result);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new UpstreamException(
                $"Yahoo Finance named a time zone that could not be resolved for '{symbol}'.", ex);
        }
    }

    private static IReadOnlyList<IntradayBar> MapBars(YahooChartResult result)
    {
        IReadOnlyList<long>? timestamps = result.Timestamp;
        YahooChartQuote? quote = result.Indicators?.Quote is { Count: > 0 } quotes ? quotes[0] : null;

        if (timestamps is null || quote is null)
        {
            return [];
        }

        TimeZoneInfo exchangeZone = ResolveExchangeZone(result.Meta);

        List<IntradayBar> bars = new(timestamps.Count);
        for (int i = 0; i < timestamps.Count; i++)
        {
            decimal? high = ValueAt(quote.High, i);
            decimal? low = ValueAt(quote.Low, i);
            long? volume = ValueAt(quote.Volume, i);

            // Yahoo pads gaps (halts, missing prints) with nulls; drop those bars.
            if (high is null || low is null || volume is null)
            {
                continue;
            }

            DateTimeOffset timestamp = TimeZoneInfo.ConvertTime(
                DateTimeOffset.FromUnixTimeSeconds(timestamps[i]), exchangeZone);

            bars.Add(new IntradayBar(timestamp, high.Value, low.Value, volume.Value));
        }

        return bars;
    }

    private static TimeZoneInfo ResolveExchangeZone(YahooChartMeta? meta)
    {
        if (meta?.ExchangeTimezoneName is { } id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        // No named zone: the scalar gmtoffset is all we have. Fixed offset, no DST.
        TimeSpan offset = TimeSpan.FromSeconds(meta?.Gmtoffset ?? 0);
        return TimeZoneInfo.CreateCustomTimeZone("yahoo-gmtoffset", offset, "Yahoo gmtoffset", "Yahoo gmtoffset");
    }

    private static T? ValueAt<T>(IReadOnlyList<T?>? values, int index) where T : struct =>
        values is not null && index < values.Count ? values[index] : null;
}
