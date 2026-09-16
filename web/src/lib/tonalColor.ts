/** `rgba(r,g,b,.14)` background plus the literal hex as the text colour - the
 *  tonal badge treatment used for the modality/division/category chips. */
export function tonalStyle(hex: string): string {
  const r = parseInt(hex.slice(1, 3), 16)
  const g = parseInt(hex.slice(3, 5), 16)
  const b = parseInt(hex.slice(5, 7), 16)
  return `background:rgba(${r},${g},${b},.14);color:${hex}`
}
