export type FootballType = 'Football11' | 'Football8' | 'Futsal' | 'BeachSoccer'
export type Division = 'HonorDivision' | 'RegionalLeague' | 'First' | 'Second' | 'Third'
export type AgeCategory =
  | 'Chupetes'
  | 'Prebenjamines'
  | 'Benjamines'
  | 'Alevines'
  | 'Infantiles'
  | 'Cadetes'
  | 'Juveniles'
  | 'Aficionados'
  | 'Veteranos'
export type PitchSurface = 'NaturalGrass' | 'ArtificialTurf' | 'Hybrid' | 'Earth' | 'Indoor'

/** Optional "ficha" fields a team may carry, mirrored on create/update/DTO. */
export interface TeamProfileFields {
  shortName?: string | null
  foundedYear?: number | null
  venueName?: string | null
  venueAddress?: string | null
  venueSurface?: PitchSurface | null
  venueMapsUrl?: string | null
  colorPrimary?: string | null
  colorSecondary?: string | null
  contactEmail?: string | null
  contactPhone?: string | null
  website?: string | null
}

/** Mirrors the backend `TeamDto`. */
export interface Team extends TeamProfileFields {
  id: string
  name: string
  coach: string
  city: string
  type: FootballType
  division: Division
  category: AgeCategory
  hasCrest: boolean
  createdAt: string
  active: boolean
}

interface TeamCorePayload {
  name: string
  coach: string
  city: string
  type: FootballType
  division: Division
  category: AgeCategory
}

export type CreateTeamPayload = TeamCorePayload & TeamProfileFields
export type UpdateTeamPayload = TeamCorePayload & TeamProfileFields

export const FOOTBALL_TYPES: FootballType[] = ['Football11', 'Football8', 'Futsal', 'BeachSoccer']
export const DIVISIONS: Division[] = ['HonorDivision', 'RegionalLeague', 'First', 'Second', 'Third']
export const AGE_CATEGORIES: AgeCategory[] = [
  'Chupetes',
  'Prebenjamines',
  'Benjamines',
  'Alevines',
  'Infantiles',
  'Cadetes',
  'Juveniles',
  'Aficionados',
  'Veteranos',
]
export const PITCH_SURFACES: PitchSurface[] = [
  'NaturalGrass',
  'ArtificialTurf',
  'Hybrid',
  'Earth',
  'Indoor',
]
