import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { SymbolSearch } from './SymbolSearch'

describe('SymbolSearch', () => {
  it('submits the value with surrounding whitespace stripped', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<SymbolSearch onSubmit={onSubmit} />)

    await user.type(screen.getByRole('textbox'), '  tsla  ')
    await user.click(screen.getByRole('button'))

    expect(onSubmit).toHaveBeenCalledTimes(1)
    expect(onSubmit).toHaveBeenCalledWith('tsla')
  })

  it('never submits an empty or whitespace-only value', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<SymbolSearch onSubmit={onSubmit} />)

    const input = screen.getByRole('textbox')

    await user.type(input, '{Enter}') // empty
    await user.type(input, '   {Enter}') // whitespace only

    expect(onSubmit).not.toHaveBeenCalled()
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('disables the input and the button while busy', () => {
    render(<SymbolSearch onSubmit={vi.fn()} busy />)

    expect(screen.getByRole('textbox')).toBeDisabled()
    expect(screen.getByRole('button')).toBeDisabled()
  })
})
