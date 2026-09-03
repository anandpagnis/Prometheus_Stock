// Shared number formatting. Instances are built once at module load; locale
// follows the runtime default.

const price = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const volume = new Intl.NumberFormat(undefined, {
  useGrouping: true,
  maximumFractionDigits: 0,
})

const signedPercent = new Intl.NumberFormat(undefined, {
  style: 'percent',
  signDisplay: 'exceptZero',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

/** Price-like value at 2 decimal places. */
export function formatPrice(value: number): string {
  return price.format(value)
}

/** Whole share count with grouping separators, e.g. `49,073,348`. */
export function formatVolume(value: number): string {
  return volume.format(value)
}

/** A ratio (e.g. `0.0324`) as a signed percentage: `+3.24%`, `-1.10%`, `0.00%`. */
export function formatSignedPercent(ratio: number): string {
  return signedPercent.format(ratio)
}
