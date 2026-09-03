using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http.HttpResults;

using PrometheusStock.Api.MarketData;

namespace PrometheusStock.Api.Features.Intraday;

/// <summary>
/// <c>GET /api/stocks/{symbol}/intraday</c> — the last month of 15-minute bars for a
/// symbol, rolled up per exchange-local day.
/// </summary>
public static partial class IntradayEndpoints
{
    // Letters, digits and the punctuation real tickers use: BRK-B, ^GSPC, BF.B, ES=F.
    [GeneratedRegex(@"^[A-Za-z0-9.\-^=]{1,15}$")]
    private static partial Regex SymbolPattern();

    public static IEndpointRouteBuilder MapIntradayEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stocks/{symbol}/intraday", GetIntradayAsync)
            .WithName("GetIntraday");

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<IntradayResponse>>, ValidationProblem>> GetIntradayAsync(
        string symbol,
        IStockDataProvider provider,
        IIntradayAggregator aggregator,
        CancellationToken cancellationToken)
    {
        if (!SymbolPattern().IsMatch(symbol))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["symbol"] = [$"'{symbol}' is not a valid ticker symbol."],
            });
        }

        IReadOnlyList<IntradayBar> bars = await provider.GetIntradayBarsAsync(symbol, cancellationToken);
        IReadOnlyList<DailyAggregate> daily = aggregator.Aggregate(bars);

        IReadOnlyList<IntradayResponse> body = [.. daily.Select(IntradayResponse.FromAggregate)];
        return TypedResults.Ok(body);
    }
}
