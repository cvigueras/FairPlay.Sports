export type FootballType = 'Football11' | 'Football8' | 'Futsal' | 'BeachSoccer'
export type Division = 'HonorDivision' | 'RegionalLeague' | 'First' | 'Second'
export type AgeCategory =
  | 'Under19'
  | 'Under16'
  | 'Under14'
  | 'Under12'
  | 'Under10'
  | 'Under8'
  | 'Under6'

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
  'Under19',
  'Under16',
  'Under14',
  'Under12',
  'Under10',
  'Under8',
  'Under6',
]
