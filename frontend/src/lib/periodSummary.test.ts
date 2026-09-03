import { describe, expect, it } from 'vitest'

import { type DailySummary } from '../api/types'
import { computePeriodSummary } from './periodSummary'

const row = (day: string, low: number, high: number, volume: number): DailySummary => ({
  day,
  lowAverage: low,
  highAverage: high,
  volume,
})

describe('computePeriodSummary', () => {
  it('derives every metric across a multi-day window', () => {
    const s = computePeriodSummary([
      row('2026-01-05', 10, 12, 1000), // mid 11
      row('2026-01-06', 8, 20, 3000), // spread 12 — widest
      row('2026-01-07', 11, 13, 2000), // mid 12
    ])

    expect(s.periodHigh).toBe(20)
    expect(s.periodLow).toBe(8)
    expect(s.totalVolume).toBe(6000)
    expect(s.averageDailyVolume).toBe(2000)
    expect(s.tradingDays).toBe(3)
    expect(s.widestDay).toEqual({ day: '2026-01-06', spread: 12 })
    expect(s.netDrift).toEqual({ ratio: 1 / 11, direction: 'up' }) // mid 11 → 12
  })

  it('handles a single-day window', () => {
    const s = computePeriodSummary([row('2026-01-05', 40, 50, 100)])

    expect(s.periodHigh).toBe(50)
    expect(s.periodLow).toBe(40)
    expect(s.tradingDays).toBe(1)
    expect(s.averageDailyVolume).toBe(100)
    expect(s.widestDay).toEqual({ day: '2026-01-05', spread: 10 })
    expect(s.netDrift).toEqual({ ratio: 0, direction: 'flat' })
  })

  it('reports a downward drift', () => {
    const s = computePeriodSummary([
      row('2026-01-05', 90, 110, 1), // mid 100
      row('2026-01-06', 45, 55, 1), // mid 50
    ])
    expect(s.netDrift).toEqual({ ratio: -0.5, direction: 'down' })
  })

  it('keeps the earliest day when spreads tie', () => {
    const s = computePeriodSummary([
      row('2026-01-05', 10, 15, 1),
      row('2026-01-06', 20, 25, 1),
    ])
    expect(s.widestDay.day).toBe('2026-01-05')
  })

  it('returns null drift when the first midpoint is zero', () => {
    const s = computePeriodSummary([
      row('2026-01-05', 0, 0, 1),
      row('2026-01-06', 1, 3, 1),
    ])
    expect(s.netDrift).toBeNull()
  })

  it('throws on an empty window', () => {
    expect(() => computePeriodSummary([])).toThrow()
  })
})
