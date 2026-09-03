import { setupServer } from 'msw/node'

import { handlers } from './handlers'

// Node-side MSW server consumed by the Vitest setup file.
export const server = setupServer(...handlers)
