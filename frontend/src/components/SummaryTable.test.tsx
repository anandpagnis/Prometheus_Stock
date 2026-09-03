import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { type DailySummary } from '../api/types'
import { SummaryTable } from './SummaryTable'

const rows: DailySummary[] = [
  { day: '2009-01-30', lowAverage: 40.2958, highAverage: 49.7534, volume: 49073348 },
  { day: '2009-02-02', lowAverage: 1, highAverage: 2.5, volume: 1000 },
]

describe('SummaryTable', () => {
  it('captions the table with the symbol', () => {
    render(<SummaryTable symbol="TSLA" rows={rows} />)
    expect(screen.getByRole('table', { name: /TSLA/ })).toBeInTheDocument()
  })

  it('has the four expected column headers and a row header per day', () => {
    render(<SummaryTable symbol="TSLA" rows={rows} />)

    for (const name of ['Day', 'Low avg', 'High avg', 'Volume']) {
      expect(screen.getByRole('columnheader', { name })).toBeInTheDocument()
    }
    expect(screen.getByRole('rowheader', { name: '2009-01-30' })).toBeInTheDocument()
    expect(screen.getByRole('rowheader', { name: '2009-02-02' })).toBeInTheDocument()
  })

  it('formats prices to four fraction digits and groups volume', () => {
    render(<SummaryTable symbol="TSLA" rows={rows} />)

    expect(screen.getByRole('cell', { name: '40.2958' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '49.7534' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '1.0000' })).toBeInTheDocument() // trailing zeros kept
    expect(screen.getByRole('cell', { name: '49,073,348' })).toBeInTheDocument()
  })

  it('shows an empty state when there are no rows', () => {
    render(<SummaryTable symbol="TSLA" rows={[]} />)
    expect(screen.getByText(/no data available for TSLA/i)).toBeInTheDocument()
  })
})
