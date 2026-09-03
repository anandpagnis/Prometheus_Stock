export interface DailySummary {
  day: string
  lowAverage: number
  highAverage: number
  volume: number
}

export class SymbolNotFoundError extends Error {
  readonly symbol: string
  constructor(symbol: string) {
    super(`Symbol '${symbol}' not found`)
    this.name = 'SymbolNotFoundError'
    this.symbol = symbol
  }
}

export class RequestFailedError extends Error {
  constructor(message = 'The request failed') {
    super(message)
    this.name = 'RequestFailedError'
  }
}