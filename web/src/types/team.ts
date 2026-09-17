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

/** How the kit's two colours are laid out on the shirt. */
export type KitPattern =
  | 'Plain'
  | 'Stripes'
  | 'Hoops'
  | 'FullHoops'
  | 'Halves'
  | 'Sash'
  | 'Checkered'
  | 'Sleeves'
  | 'Fade'

/** The capacity a user takes part in a team as, chosen when founding/joining it. */
export type TeamMemberRole = 'Delegate' | 'Coach' | 'President' | 'TechnicalStaff' | 'Player'

export const TEAM_MEMBER_ROLES: TeamMemberRole[] = [
  'Delegate',
  'Coach',
  'President',
  'TechnicalStaff',
  'Player',
]

/** Mirrors the backend `TeamMemberDto` - one user's membership in one team. */
export interface TeamMembership {
  id: string
  teamId: string
  userId: string
  role: TeamMemberRole
  displayName: string
  createdAt: string
}

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
  kitPattern?: KitPattern | null
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
export const KIT_PATTERNS: KitPattern[] = [
  'Plain',
  'Stripes',
  'Hoops',
  'FullHoops',
  'Halves',
  'Sash',
  'Checkered',
  'Sleeves',
  'Fade',
]
