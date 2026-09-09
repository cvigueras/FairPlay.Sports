export type FootballType = 'Football11' | 'Football8' | 'Futsal' | 'BeachSoccer'
export type Division = 'HonorDivision' | 'RegionalLeague' | 'First' | 'Second'
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

/** Mirrors the backend `TeamDto`. */
export interface Team {
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

export interface CreateTeamPayload {
  name: string
  coach: string
  city: string
  type: FootballType
  division: Division
  category: AgeCategory
}

export const FOOTBALL_TYPES: FootballType[] = ['Football11', 'Football8', 'Futsal', 'BeachSoccer']
export const DIVISIONS: Division[] = ['HonorDivision', 'RegionalLeague', 'First', 'Second']
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
