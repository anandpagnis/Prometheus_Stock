import { type FormEvent, useState } from 'react'

export interface SymbolSearchProps {
  /** Called with the trimmed, non-empty symbol when the form is submitted. */
  onSubmit: (symbol: string) => void
  /** When true the form is inert — a lookup is already in flight. */
  busy?: boolean
}

/**
 * Single-field form for entering a stock symbol. Trims the surrounding
 * whitespace, never fires `onSubmit` with an empty string, and disables itself
 * while `busy`. Casing is left to the caller (the API layer upper-cases).
 */
export function SymbolSearch({ onSubmit, busy = false }: SymbolSearchProps) {
  const [value, setValue] = useState('')
  const symbol = value.trim()
  const canSubmit = !busy && symbol !== ''

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canSubmit) return
    onSubmit(symbol)
  }

  return (
    <form onSubmit={handleSubmit} role="search">
      <label htmlFor="symbol-search">Stock symbol</label>
      <input
        id="symbol-search"
        name="symbol"
        value={value}
        onChange={(event) => setValue(event.target.value)}
        placeholder="e.g. TSLA"
        autoComplete="off"
        disabled={busy}
      />
      <button type="submit" disabled={!canSubmit}>
        {busy ? 'Loading…' : 'Search'}
      </button>
    </form>
  )
}
