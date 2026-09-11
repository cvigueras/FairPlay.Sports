import type { PitchSurface } from '@/types/team'

/**
 * A colour per pitch surface, kept in the same Material ~700 family as
 * AGE_CATEGORY_COLOR, DIVISION_COLOR and MODALITY_COLOR so the palettes sit
 * together.
 */
export const SURFACE_COLOR: Record<PitchSurface, string> = {
  NaturalGrass: '#2E7D32', // grass green
  ArtificialTurf: '#00897B', // synthetic teal
  Hybrid: '#9E9D24', // olive — natural/artificial blend
  Earth: '#8D6E63', // dirt brown
  Indoor: '#1565C0', // indoor-court blue
}
