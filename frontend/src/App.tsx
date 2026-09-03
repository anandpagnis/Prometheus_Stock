import { IntradayResults } from './components/IntradayResults'
import { SymbolSearch } from './components/SymbolSearch'
import { useIntradayData } from './hooks/useIntradayData'

export default function App() {
  const { state, load } = useIntradayData()

  return (
    <main>
      <h1>Prometheus Stock Dashboard - Take Home Assessment - Anand Pagnis</h1>
      <p>Intraday high/low averages and volume, grouped by trading day.</p>
      <SymbolSearch onSubmit={load} busy={state.status === 'loading'} />
      <IntradayResults state={state} />
    </main>
  )
}