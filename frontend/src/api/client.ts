import { type DailySummary, RequestFailedError, SymbolNotFoundError } from './types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5136'

export async function fetchIntraday(symbol: string): Promise<DailySummary[]> {
  const normalized = symbol.trim().toUpperCase()

  let response: Response
  try {
    response = await fetch(
      `${BASE_URL}/api/stocks/${encodeURIComponent(normalized)}/intraday`,
      { headers: { Accept: 'application/json' } },
    )
  } catch {
    throw new RequestFailedError('Could not reach the server')
  }

  if (response.status === 404) throw new SymbolNotFoundError(normalized)
  if (!response.ok) throw new RequestFailedError(`Server responded ${response.status}`)

  try {
    return (await response.json()) as DailySummary[]
  } catch {
    throw new RequestFailedError('The server response could not be read')
  }
}