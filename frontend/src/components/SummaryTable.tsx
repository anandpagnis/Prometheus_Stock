import { type DailySummary } from '../api/types'

const priceFormat = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 4,
  maximumFractionDigits: 4,
})

const volumeFormat = new Intl.NumberFormat(undefined, {
  useGrouping: true,
  maximumFractionDigits: 0,
})

export interface SummaryTableProps {
  symbol: string
  rows: DailySummary[]
}

/**
 * Renders the per-day intraday summary as a table. Prices are shown at 4 fraction
 * digits; volume is grouped. Locale follows the runtime default.
 */
export function SummaryTable({ symbol, rows }: SummaryTableProps) {
  return (
    <table>
      <caption>Daily intraday summary for {symbol}</caption>
      <thead>
        <tr>
          <th scope="col">Day</th>
          <th scope="col">Low avg</th>
          <th scope="col">High avg</th>
          <th scope="col">Volume</th>
        </tr>
      </thead>
      <tbody>
        {rows.length === 0 ? (
          <tr>
            <td colSpan={4}>No data available for {symbol}.</td>
          </tr>
        ) : (
          rows.map((row) => (
            <tr key={row.day}>
              <th scope="row">{row.day}</th>
              <td>{priceFormat.format(row.lowAverage)}</td>
              <td>{priceFormat.format(row.highAverage)}</td>
              <td>{volumeFormat.format(row.volume)}</td>
            </tr>
          ))
        )}
      </tbody>
    </table>
  )
}
