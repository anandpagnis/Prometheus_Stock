import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import type { DailySummary } from '../api/types'

export function SummaryChart({ symbol, rows }: { symbol: string; rows: DailySummary[] }) {
  return (
    <figure
      data-testid="summary-chart"
      aria-label={`${symbol} daily high and low averages over time`}
      style={{ margin: 0 }}
    >
      <ResponsiveContainer width="100%" height={320}>
        <LineChart data={rows} margin={{ top: 8, right: 16, bottom: 8, left: 0 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="day" />
          <YAxis domain={['auto', 'auto']} width={72} />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey="lowAverage" name="Low avg" stroke="#2563eb" dot={false} />
          <Line type="monotone" dataKey="highAverage" name="High avg" stroke="#dc2626" dot={false} />
        </LineChart>
      </ResponsiveContainer>
    </figure>
  )
}