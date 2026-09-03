import '@testing-library/jest-dom/vitest'

import { afterAll, afterEach, beforeAll } from 'vitest'

import { server } from '../mocks/server'

// Start the mock server before the suite; fail loudly on any unhandled request.
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))

// Drop per-test handler overrides so tests stay isolated.
afterEach(() => server.resetHandlers())

// Tear the mock server down once the suite finishes.
afterAll(() => server.close())
