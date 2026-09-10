import type { AgeCategory } from '@/types/team'

/**
 * A colour per age category, ordered youngest to oldest as a cool-to-warm
 * ramp. Kept in the same Material ~700 family as MODALITY_COLOR so the two
 * palettes sit together, closing on the sand tone shared with Beach soccer.
 */
export const AGE_CATEGORY_COLOR: Record<AgeCategory, string> = {
  Chupetes: '#00897B', // teal
  Prebenjamines: '#0277BD', // light blue
  Benjamines: '#1565C0', // blue (shared with Futsal)
  Alevines: '#5E35B1', // deep purple
  Infantiles: '#8E24AA', // purple
  Cadetes: '#AD1457', // pink
  Juveniles: '#EF6C00', // orange (near 8-a-side)
  Aficionados: '#2E7D32', // green (shared with 11-a-side)
  Veteranos: '#795548', // brown (near Beach soccer sand)
}
