# Prometheus Stock — frontend

React 19 + Vite + TypeScript SPA for the intraday endpoint. See the [repository
README](../README.md) for full setup and run instructions.

```bash
npm install
npm run dev      # http://localhost:5173
npm test         # vitest
npm run lint
npm run build    # tsc -b && vite build
```

`VITE_API_BASE_URL` (see `.env.example`) overrides the backend URL; it defaults to
`http://localhost:5136`.
