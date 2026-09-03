namespace PrometheusStock.Api.MarketData;

/// <summary>
/// Rolls a flat sequence of intraday bars up into one <see cref="DailyAggregate"/>
/// per exchange-local calendar day. Implementations must be pure and deterministic
/// so they can be unit-tested without any test doubles.
/// </summary>
public interface IIntradayAggregator
{
    /// <param name="bars">
    /// Intraday bars in any order; may be empty. Bars are grouped by the calendar
    /// date of their <see cref="IntradayBar.Timestamp"/>.
    /// </param>
    /// <returns>
    /// One aggregate per day that had at least one bar, ordered by
    /// <see cref="DailyAggregate.Day"/> ascending. Empty input yields an empty list.
    /// </returns>
    IReadOnlyList<DailyAggregate> Aggregate(IReadOnlyList<IntradayBar> bars);
}
