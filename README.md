# Prometheus Stock

A full-stack app that reads intraday market data from Yahoo Finance, rolls it up by
trading day, and shows it as a table and a chart.

- **Backend** — ASP.NET Core (.NET 10) minimal API. One endpoint,
  `GET /api/stocks/{symbol}/intraday`: the last month of 15-minute bars for a symbol,
  grouped per exchange-local day.
- **Frontend** — React 19 + Vite + TypeScript SPA that consumes that endpoint.

## Prerequisites

| Tool     | Version              |
| -------- | ------------------- |
| .NET SDK | 10.0+               |
| Node.js  | 20.19+ or 22.12+    |
| npm      | 10+                 |

Everything runs locally; the backend reaches out to the public Yahoo Finance chart API,
so an internet connection is required.

## Run it

### 1 — Backend (terminal 1)

```bash
cd backend
dotnet run --project src/PrometheusStock.Api
```

Serves on **http://localhost:5136**. Quick check:

```bash
curl http://localhost:5136/api/stocks/TSLA/intraday
```

### 2 — Frontend (terminal 2)

```bash
cd frontend
npm install
npm run dev
```

Serves on **http://localhost:5173** (Vite falls forward to 5174, … if 5173 is taken —
both are in the backend's CORS allow-list). Open it, enter a symbol (`TSLA`, `AAPL`,
`BRK-B`, `^GSPC`, …) and submit.

To point the SPA at a different backend, copy `frontend/.env.example` to
`frontend/.env` and set `VITE_API_BASE_URL`.

## API

### `GET /api/stocks/{symbol}/intraday`

`symbol` — 1–15 characters of `[A-Za-z0-9.\-^=]`.

**200** → JSON array, one object per trading day, ascending by day:

```json
[
  { "day": "2009-01-30", "lowAverage": 40.2958, "highAverage": 49.7534, "volume": 49073348 }
]
```

- `day` — exchange-local calendar day, `yyyy-MM-dd`
- `lowAverage` / `highAverage` — mean of the interval lows / highs that day, 4 decimal places (banker's rounding)
- `volume` — total shares traded that day

| Situation                        | Status | Body                                     |
| -------------------------------- | ------ | ---------------------------------------- |
| symbol fails the format rule     | `400`  | `application/problem+json`               |
| Yahoo has no data for the symbol | `404`  | `application/problem+json`               |
| Yahoo unreachable / erroring     | `502`  | `application/problem+json` (no detail)   |

`GET /health` → `{ "status": "healthy" }`.

In development, OpenAPI is served at `/openapi/v1.json`.

## Tests

```bash
# backend — xUnit: pure unit + WireMock (Yahoo client) + WebApplicationFactory (endpoint)
cd backend  && dotnet test

# frontend — Vitest: component tests + MSW-backed App integration tests
cd frontend && npm test
               npm run lint
               npm run build
```

## Layout

```
backend/src/PrometheusStock.Api/
  Program.cs                    pipeline + composition root
  Features/Intraday/            the endpoint + its rounded response DTO
  MarketData/
    IntradayBar, DailyAggregate domain records
    IIntradayAggregator         pure per-day roll-up (mean low/high, sum volume)
    IStockDataProvider          seam over the market-data source
    Yahoo/YahooFinanceClient    typed HttpClient + internal wire DTOs + per-bar
                                TimeZoneInfo.ConvertTime (DST-correct day bucketing)
  Common/                       problem+json IExceptionHandler
frontend/src/
  api/                          fetch, error types, symbol normalisation
  hooks/useIntradayData         idle / loading / success / error state machine
  components/                   SymbolSearch, SummaryTable, SummaryChart (lazy-loaded),
                                IntradayResults
```

## Scope

An MVP built to grow. Deliberately deferred: retry / circuit-breaker on the Yahoo call,
response caching, API versioning, auth, structured request logging. Configuration:
`YahooFinance:*` (base URL, User-Agent, range, interval) and `Cors:AllowedOrigins` in
`backend/src/PrometheusStock.Api/appsettings.json`.

`PROMPT_LOG.md` — the prompt-by-prompt AI-collaboration log.
`NOTES.md` — changes made by hand, outside the AI loop.
