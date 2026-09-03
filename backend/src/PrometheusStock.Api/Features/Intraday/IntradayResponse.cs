using System.Globalization;

using PrometheusStock.Api.MarketData;

namespace PrometheusStock.Api.Features.Intraday;

/// <summary>
/// One day of the <c>GET /api/stocks/{symbol}/intraday</c> payload. Averages are
/// rounded to 4 decimal places (banker's rounding) here, at the API boundary;
/// <see cref="DailyAggregate" /> itself stays full precision.
/// </summary>
public sealed record IntradayResponse(string Day, decimal LowAverage, decimal HighAverage, long Volume)
{
    public static IntradayResponse FromAggregate(DailyAggregate aggregate) => new(
        aggregate.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Math.Round(aggregate.LowAverage, 4, MidpointRounding.ToEven),
        Math.Round(aggregate.HighAverage, 4, MidpointRounding.ToEven),
        aggregate.Volume);
}
