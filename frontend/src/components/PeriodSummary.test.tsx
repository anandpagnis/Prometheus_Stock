import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { type DailySummary } from '../api/types'
import { PeriodSummary } from './PeriodSummary'

const row = (day: string, low: number, high: number, volume: number): DailySummary => ({
  day,
  lowAverage: low,
  highAverage: high,
  volume,
})

// each stat renders as <div><dt>{label}</dt><dd>{value}</dd></div>
const value = (label: string) => screen.getByText(label).nextElementSibling

describe('PeriodSummary', () => {
  it('renders every derived figure', () => {
    render(
      <PeriodSummary
        symbol="TSLA"
        rows={[
          row('2026-01-05', 100, 110, 1_000_000), // mid 105
          row('2026-01-06', 96, 130, 3_000_000), // spread 34 — widest
          row('2026-01-07', 108, 116, 2_000_000), // mid 112
        ]}
      />,
    )

    expect(screen.getByRole('region', { name: /TSLA period summary/i })).toBeInTheDocument()
    expect(value('Period high')).toHaveTextContent('130.00')
    expect(value('Period low')).toHaveTextContent('96.00')
    expect(value('Total volume')).toHaveTextContent('6,000,000')
    expect(value('Avg daily volume')).toHaveTextContent('2,000,000')
    expect(value('Trading days')).toHaveTextContent('3')
    expect(value('Widest day')).toHaveTextContent('34.00')
    expect(value('Widest day')).toHaveTextContent('2026-01-06')
    expect(value('Net drift')).toHaveTextContent('+6.67%') // mid 105 → 112
    expect(value('Net drift')).toHaveAttribute('data-direction', 'up')
  })

  it('marks a downward drift', () => {
    render(
      <PeriodSummary
        symbol="X"
        rows={[row('2026-01-05', 90, 110, 1), row('2026-01-06', 45, 55, 1)]} // mid 100 → 50
      />,
    )

    expect(value('Net drift')).toHaveTextContent('-50.00%')
    expect(value('Net drift')).toHaveAttribute('data-direction', 'down')
  })

  it('shows an em dash when the first midpoint is zero', () => {
    render(
      <PeriodSummary symbol="X" rows={[row('2026-01-05', 0, 0, 1), row('2026-01-06', 1, 3, 1)]} />,
    )

    expect(value('Net drift')).toHaveTextContent('—')
    expect(value('Net drift')).not.toHaveAttribute('data-direction')
  })
})
