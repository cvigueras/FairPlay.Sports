/** Curated palette for the kit colour pickers - the backend stores colours as
 *  free text (hex or name, see `KitColors`), but the wizard only offers this
 *  fixed set so users pick a swatch instead of typing/seeing a hex code. */
export interface KitColorOption {
  value: string
  labelKey: string
}

export const KIT_COLOR_PALETTE: KitColorOption[] = [
  { value: '#ffffff', labelKey: 'white' },
  { value: '#111111', labelKey: 'black' },
  { value: '#dc2626', labelKey: 'red' },
  { value: '#7f1d1d', labelKey: 'maroon' },
  { value: '#f97316', labelKey: 'orange' },
  { value: '#facc15', labelKey: 'yellow' },
  { value: '#16a34a', labelKey: 'green' },
  { value: '#0d9488', labelKey: 'teal' },
  { value: '#0ea5e9', labelKey: 'skyBlue' },
  { value: '#2563eb', labelKey: 'blue' },
  { value: '#1e3a8a', labelKey: 'navy' },
  { value: '#7c3aed', labelKey: 'purple' },
  { value: '#db2777', labelKey: 'pink' },
  { value: '#64748b', labelKey: 'grey' },
]
