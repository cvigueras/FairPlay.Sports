/** "hace 2 días" / "in 3 hours" style formatting, scaling the unit to the gap. */
export function relativeTime(date: Date, locale: string): string {
  const diffMs = date.getTime() - Date.now()
  const diffHours = Math.round(diffMs / 3_600_000)
  const diffDays = Math.round(diffMs / 86_400_000)
  const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' })
  if (Math.abs(diffHours) < 24) return rtf.format(diffHours, 'hour')
  if (Math.abs(diffDays) < 30) return rtf.format(diffDays, 'day')
  return rtf.format(Math.round(diffDays / 30), 'month')
}
