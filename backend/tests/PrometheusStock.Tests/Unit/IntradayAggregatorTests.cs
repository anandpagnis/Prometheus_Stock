using PrometheusStock.Api.MarketData;

namespace PrometheusStock.Tests.Unit;

/// <summary>
/// Behaviour spec for <see cref="IntradayAggregator" />: group intraday bars by the
/// bar's own local calendar date, mean-average the lows and highs at full decimal
/// precision, sum the volume, and return the days in ascending order.
/// </summary>
public sealed class IntradayAggregatorTests
{
    [Fact]
    public void Empty_input_returns_an_empty_list() =>
        Aggregate().ShouldBeEmpty();

    [Fact]
    public void A_single_bar_yields_one_aggregate_matching_that_bar()
    {
        IntradayBar bar = Bar(At(2026, 8, 28, 9, 30), low: 99.25m, high: 101.5m, volume: 12_345);

        DailyAggregate day = Aggregate(bar).ShouldHaveSingleItem();

        day.Day.ShouldBe(new DateOnly(2026, 8, 28));
        day.LowAverage.ShouldBe(99.25m);
        day.HighAverage.ShouldBe(101.5m);
        day.Volume.ShouldBe(12_345L);
    }

    [Fact]
    public void Bars_in_a_day_are_mean_averaged_and_volume_summed()
    {
        DateTimeOffset d = At(2026, 8, 28, 0);

        DailyAggregate day = Aggregate(
            Bar(d.AddHours(10), low: 10m, high: 11m, volume: 100),
            Bar(d.AddHours(11), low: 20m, high: 22m, volume: 200),
            Bar(d.AddHours(12), low: 30m, high: 33m, volume: 300)).ShouldHaveSingleItem();

        day.LowAverage.ShouldBe(20m);   // (10 + 20 + 30) / 3
        day.HighAverage.ShouldBe(22m);  // (11 + 22 + 33) / 3
        day.Volume.ShouldBe(600L);
    }

    [Fact]
    public void Averages_are_not_rounded()
    {
        DateTimeOffset d = At(2026, 8, 28, 14);

        DailyAggregate day = Aggregate(
            Bar(d, low: 10m, high: 100m),
            Bar(d.AddMinutes(15), low: 20m, high: 100m),
            Bar(d.AddMinutes(30), low: 1m, high: 100m)).ShouldHaveSingleItem();

        // (10 + 20 + 1) / 3 = 10.333… kept to full decimal precision, not rounded.
        day.LowAverage.ShouldBe(31m / 3m);
    }

    [Fact]
    public void Aggregates_are_ordered_by_day_ascending()
    {
        IReadOnlyList<DailyAggregate> result = Aggregate(
            Bar(At(2026, 8, 31, 10), low: 4m, high: 5m),
            Bar(At(2026, 8, 28, 10), low: 4m, high: 5m),
            Bar(At(2026, 9, 2, 10), low: 4m, high: 5m),
            Bar(At(2026, 8, 28, 15), low: 4m, high: 5m));

        result.Select(a => a.Day).ShouldBe(
        [
            new DateOnly(2026, 8, 28),
            new DateOnly(2026, 8, 31),
            new DateOnly(2026, 9, 2),
        ]);
    }

    [Fact]
    public void Bar_order_within_a_day_does_not_change_the_result()
    {
        DateTimeOffset d = At(2026, 8, 28, 0);
        IntradayBar a = Bar(d.AddHours(9), low: 10m, high: 12m, volume: 100);
        IntradayBar b = Bar(d.AddHours(12), low: 14m, high: 18m, volume: 250);
        IntradayBar c = Bar(d.AddHours(15), low: 17m, high: 21m, volume: 300);

        DailyAggregate ordered = Aggregate(a, b, c).ShouldHaveSingleItem();
        DailyAggregate shuffled = Aggregate(c, a, b).ShouldHaveSingleItem();

        shuffled.ShouldBe(ordered);
    }

    [Fact]
    public void Days_are_taken_from_the_bar_local_offset_not_utc()
    {
        // 23:30 at -04:00 is still the 28th locally, though 03:30 UTC on the 29th.
        IntradayBar lateOn28 = Bar(At(2026, 8, 28, 23, 30), low: 10m, high: 10m);
        IntradayBar earlyOn29 = Bar(At(2026, 8, 29, 0, 30), low: 20m, high: 20m);

        IReadOnlyList<DailyAggregate> result = Aggregate(lateOn28, earlyOn29);

        result.Select(a => a.Day).ShouldBe([new DateOnly(2026, 8, 28), new DateOnly(2026, 8, 29)]);
        result[0].LowAverage.ShouldBe(10m);
        result[1].LowAverage.ShouldBe(20m);
    }

    // -- fixture --------------------------------------------------------------

    private static readonly TimeSpan Edt = TimeSpan.FromHours(-4);

    private static DateTimeOffset At(int year, int month, int day, int hour, int minute = 0) =>
        new(year, month, day, hour, minute, 0, Edt);

    private static IntradayBar Bar(DateTimeOffset timestamp, decimal low, decimal high, long volume = 1) =>
        new(timestamp, High: high, Low: low, Volume: volume);

    private static IReadOnlyList<DailyAggregate> Aggregate(params IntradayBar[] bars) =>
        new IntradayAggregator().Aggregate(bars);
}
