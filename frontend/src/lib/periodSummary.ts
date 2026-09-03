import { type DailySummary } from '../api/types'

export interface PeriodSummary {
  /** Highest daily high-average across the window. */
  periodHigh: number
  /** Lowest daily low-average across the window. */
  periodLow: number
  totalVolume: number
  averageDailyVolume: number
  /** Days that had intraday data — weekends and holidays are already absent. */
  tradingDays: number
  /** The day with the largest `highAverage − lowAverage`, and that spread. */
  widestDay: { day: string; spread: number }
  /**
   * Change in the daily midpoint `(highAverage + lowAverage) / 2` from the first
   * day to the last, as a ratio. It is a midpoint drift, not an open→close return
   * (the source only carries aggregated hi/lo averages). `null` when the first
   * midpoint is 0 (the change is undefined).
   */
  netDrift: { ratio: number; direction: 'up' | 'down' | 'flat' } | null
}

export function computePeriodSummary(rows: readonly DailySummary[]): PeriodSummary {
  if (rows.length === 0) {
    throw new Error('computePeriodSummary needs at least one row')
  }

  const first = rows[0]
  let periodHigh = first.highAverage
  let periodLow = first.lowAverage
  let totalVolume = 0
  let widestDay = { day: first.day, spread: first.highAverage - first.lowAverage }

  for (const row of rows) {
    periodHigh = Math.max(periodHigh, row.highAverage)
    periodLow = Math.min(periodLow, row.lowAverage)
    totalVolume += row.volume

    const spread = row.highAverage - row.lowAverage
    if (spread > widestDay.spread) {
      widestDay = { day: row.day, spread }
    }
  }

  const tradingDays = rows.length

  return {
    periodHigh,
    periodLow,
    totalVolume,
    averageDailyVolume: totalVolume / tradingDays,
    tradingDays,
    widestDay,
    netDrift: drift(midpoint(first), midpoint(rows[tradingDays - 1])),
  }
}

function midpoint(row: DailySummary): number {
  return (row.highAverage + row.lowAverage) / 2
}

function drift(first: number, last: number): PeriodSummary['netDrift'] {
  if (first === 0) return null
  const ratio = (last - first) / first
  return {
    ratio,
    direction: ratio > 0 ? 'up' : ratio < 0 ? 'down' : 'flat',
  }
}
