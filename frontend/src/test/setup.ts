import '@testing-library/jest-dom/vitest'

import { cleanup } from '@testing-library/react'
import { afterAll, afterEach, beforeAll } from 'vitest'

import { server } from '../mocks/server'

// Start the mock server before the suite; fail loudly on any unhandled request.
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))

// Unmount rendered trees and drop per-test handler overrides so tests stay isolated.
// (Vitest isn't running with `globals: true`, so RTL's auto-cleanup is not wired up.)
afterEach(() => {
  cleanup()
  server.resetHandlers()
})

// Tear the mock server down once the suite finishes.
afterAll(() => server.close())
