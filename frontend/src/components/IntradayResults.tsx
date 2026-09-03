import { lazy, Suspense } from 'react'

import type { IntradayState } from '../hooks/useIntradayData'
import { PeriodSummary } from './PeriodSummary'
import { SummaryTable } from './SummaryTable'

// recharts is large and only needed on the success branch — keep it out of the
// initial bundle.
const SummaryChart = lazy(() =>
  import('./SummaryChart').then((module) => ({ default: module.SummaryChart })),
)

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
          <PeriodSummary symbol={state.symbol} rows={state.rows} />
          <Suspense fallback={<p>Loading chart…</p>}>
            <SummaryChart symbol={state.symbol} rows={state.rows} />
          </Suspense>
          <SummaryTable symbol={state.symbol} rows={state.rows} />
        </>
      )
    default: {
      const exhaustive: never = state
      return exhaustive
    }
  }
}
