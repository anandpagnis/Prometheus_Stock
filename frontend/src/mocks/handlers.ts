import { http, HttpResponse } from 'msw'
import type { DailySummary } from '../api/types'

export const sampleRows: DailySummary[] = [
  { day: '2009-01-30', lowAverage: 40.2958, highAverage: 49.7534, volume: 49073348 },
  { day: '2009-02-02', lowAverage: 41.101, highAverage: 50.223, volume: 38100200 },
]

export const handlers = [
  http.get('http://localhost:5136/api/stocks/:symbol/intraday', () =>
    HttpResponse.json(sampleRows),
  ),
]