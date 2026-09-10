import type { FootballType } from '@/types/team'

/** Signature colour for each football modality, used by ModalityIcon and chips. */
export const MODALITY_COLOR: Record<FootballType, string> = {
  Football11: '#2E7D32', // grass green
  Football8: '#F57C00', // synthetic-pitch orange
  Futsal: '#1565C0', // indoor-court blue
  BeachSoccer: '#C0894A', // sand
}
