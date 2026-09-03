import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'

import App from './App'
import { server } from './mocks/server'

async function search(symbol: string) {
  const user = userEvent.setup()
  await user.type(screen.getByRole('textbox'), symbol)
  await user.click(screen.getByRole('button', { name: /search/i }))
}

describe('App', () => {
  it('shows the summary table after a successful lookup', async () => {
    server.use(
      http.get('*/api/stocks/:symbol/intraday', () =>
        HttpResponse.json([
          { day: '2009-01-30', lowAverage: 40.2958, highAverage: 49.7534, volume: 49073348 },
        ]),
      ),
    )
    render(<App />)

    await search('aapl')

    const table = await screen.findByRole('table', { name: /AAPL/ })
    expect(table).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '40.2958' })).toBeInTheDocument()
  })

  it('shows a not-found alert naming the symbol on a 404', async () => {
    server.use(
      http.get('*/api/stocks/:symbol/intraday', () => new HttpResponse(null, { status: 404 })),
    )
    render(<App />)

    await search('nope')

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('NOPE')
  })

  it('shows a generic alert on an upstream (502) failure', async () => {
    server.use(
      http.get('*/api/stocks/:symbol/intraday', () => new HttpResponse(null, { status: 502 })),
    )
    render(<App />)

    await search('tsla')

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent(/something went wrong/i)
    expect(alert).not.toHaveTextContent('TSLA')
  })
})
