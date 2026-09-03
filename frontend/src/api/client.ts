import { type DailySummary, RequestFailedError, SymbolNotFoundError } from './types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5136'

/** Canonical symbol form — sent to the API and shown in the UI. */
export function normalizeSymbol(symbol: string): string {
  return symbol.trim().toUpperCase()
}

export async function fetchIntraday(symbol: string): Promise<DailySummary[]> {
  const normalized = normalizeSymbol(symbol)

  let response: Response
  try {
    response = await fetch(
      `${BASE_URL}/api/stocks/${encodeURIComponent(normalized)}/intraday`,
      { headers: { Accept: 'application/json' } },
    )
  } catch {
    throw new RequestFailedError('Could not reach the server')
  }

  // 404 (unknown symbol) and 400 (symbol fails the API's format rules) are both
  // "that symbol won't work, try another" as far as the user is concerned.
  if (response.status === 404 || response.status === 400) {
    throw new SymbolNotFoundError(normalized)
  }
  if (!response.ok) throw new RequestFailedError(`Server responded ${response.status}`)

  try {
    return (await response.json()) as DailySummary[]
  } catch {
    throw new RequestFailedError('The server response could not be read')
  }
}
