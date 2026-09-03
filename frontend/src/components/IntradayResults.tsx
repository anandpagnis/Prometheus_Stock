import type { IntradayState } from '../hooks/useIntradayData'
import { SummaryChart } from './SummaryChart'
import { SummaryTable } from './SummaryTable'

export function IntradayResults({ state }: { state: IntradayState }) {
  switch (state.status) {
    case 'idle':
      return <p>Enter a symbol above to see its intraday summary.</p>
    case 'loading':
      return <p role="status">Loading {state.symbol}…</p>
    case 'error':
      return (
        <p role="alert">
          {state.error === 'not-found'
            ? `We couldn't find "${state.symbol}". Check the symbol and try again.`
            : 'Something went wrong fetching the data. Please try again.'}
        </p>
      )
    case 'success':
      return state.rows.length === 0 ? (
        <p>No intraday data available for {state.symbol}.</p>
      ) : (
        <>
          <SummaryChart symbol={state.symbol} rows={state.rows} />
          <SummaryTable symbol={state.symbol} rows={state.rows} />
        </>
      )
  }
}