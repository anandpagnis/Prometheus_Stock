import { useMemo } from 'react'

import { type DailySummary } from '../api/types'
import { formatPrice, formatSignedPercent, formatVolume } from '../lib/format'
import { computePeriodSummary, type PeriodSummary as PeriodSummaryData } from '../lib/periodSummary'

/**
 * Window-level figures derived from the daily rows: highs/lows, volume, the
 * widest-range day, and the first→last midpoint drift. Assumes at least one row
 * (the parent renders an empty state otherwise).
 */
export function PeriodSummary({ symbol, rows }: { symbol: string; rows: DailySummary[] }) {
  const summary = useMemo(() => computePeriodSummary(rows), [rows])

  return (
    <section aria-label={`${symbol} period summary`}>
      <dl className="period-summary">
        <Stat label="Period high" value={formatPrice(summary.periodHigh)} />
        <Stat label="Period low" value={formatPrice(summary.periodLow)} />
        <Stat label="Total volume" value={formatVolume(summary.totalVolume)} />
        <Stat label="Avg daily volume" value={formatVolume(summary.averageDailyVolume)} />
        <Stat label="Trading days" value={String(summary.tradingDays)} />
        <Stat
          label="Widest day"
          value={formatPrice(summary.widestDay.spread)}
          sub={summary.widestDay.day}
        />
        <NetDrift drift={summary.netDrift} />
      </dl>
    </section>
  )
}

function Stat({ label, value, sub }: { label: string; value: string; sub?: string }) {
  return (
    <div className="stat">
      <dt className="stat-label">{label}</dt>
      <dd className="stat-value">
        {value}
        {sub ? <span className="stat-sub">{sub}</span> : null}
      </dd>
    </div>
  )
}

const DRIFT_ARROW: Record<'up' | 'down' | 'flat', string> = {
  up: '▲',
  down: '▼',
  flat: '→',
}

const DRIFT_TONE: Record<'up' | 'down' | 'flat', string> = {
  up: ' pos',
  down: ' neg',
  flat: '',
}

function NetDrift({ drift }: { drift: PeriodSummaryData['netDrift'] }) {
  if (drift === null) {
    return (
      <div className="stat">
        <dt className="stat-label">Net drift</dt>
        <dd className="stat-value">—</dd>
      </div>
    )
  }

  return (
    <div className="stat">
      <dt className="stat-label">Net drift</dt>
      <dd className={`stat-value${DRIFT_TONE[drift.direction]}`} data-direction={drift.direction}>
        <span aria-hidden="true">{DRIFT_ARROW[drift.direction]} </span>
        {formatSignedPercent(drift.ratio)}
      </dd>
    </div>
  )
}
