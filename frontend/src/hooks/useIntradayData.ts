import { useCallback, useRef, useState } from 'react'
import { fetchIntraday, normalizeSymbol } from '../api/client'
import { type DailySummary, SymbolNotFoundError } from '../api/types'

export type IntradayState =
  | { status: 'idle' }
  | { status: 'loading'; symbol: string }
  | { status: 'success'; symbol: string; rows: DailySummary[] }
  | { status: 'error'; symbol: string; error: 'not-found' | 'request-failed' }

export function useIntradayData() {
  const [state, setState] = useState<IntradayState>({ status: 'idle' })
  const requestId = useRef(0)

  const load = useCallback((raw: string) => {
    const symbol = normalizeSymbol(raw)
    if (symbol === '') return

    const id = ++requestId.current
    setState({ status: 'loading', symbol })

    fetchIntraday(symbol)
      .then((rows) => {
        if (id === requestId.current) setState({ status: 'success', symbol, rows })
      })
      .catch((err: unknown) => {
        if (id !== requestId.current) return
        setState({
          status: 'error',
          symbol,
          error: err instanceof SymbolNotFoundError ? 'not-found' : 'request-failed',
        })
      })
  }, [])

  return { state, load }
}