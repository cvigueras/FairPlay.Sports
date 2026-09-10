import type { Division } from '@/types/team'

/**
 * A colour per division, top tier to bottom: a gold flagship, then a
 * cool-to-warm-to-earthy descent kept in the same Material ~700 family as
 * MODALITY_COLOR and AGE_CATEGORY_COLOR so the palettes sit together.
 */
export const DIVISION_COLOR: Record<Division, string> = {
  HonorDivision: '#C9A227', // gold — top flight
  RegionalLeague: '#00838F', // dark cyan
  First: '#1565C0', // blue
  Second: '#2E7D32', // green
  Third: '#8D6E63', // brown — entry tier
}
