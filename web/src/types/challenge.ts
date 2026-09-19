import type { AgeCategory, Division, KitPattern, PitchSurface } from '@/types/team'

export type ChallengeStatus = 'Pending' | 'Accepted' | 'Rejected'
export type TeamKitSlot = 'First' | 'Second'

export interface ChallengeKit {
  colorPrimary: string
  colorSecondary: string
  shortsColor: string
  kitPattern: KitPattern
  slot: TeamKitSlot
}

/** Mirrors the backend `ChallengeDto`. */
export interface Challenge {
  id: string
  challengerTeamId: string
  challengerTeamName: string
  challengerTeamHasCrest: boolean
  challengerTeamCategory: AgeCategory
  challengerTeamDivision?: Division | null
  challengedTeamId: string
  challengedTeamName: string
  challengedTeamHasCrest: boolean
  challengedTeamCategory: AgeCategory
  challengedTeamDivision?: Division | null
  homeTeamId: string
  awayTeamId: string
  matchDate: string
  venueName?: string | null
  venueAddress?: string | null
  venueSurface?: PitchSurface | null
  venueMapsUrl?: string | null
  homeKit?: ChallengeKit | null
  awayKit?: ChallengeKit | null
  /** Both kits are known and share a primary colour - informational only, never blocking. */
  kitsClash: boolean
  message?: string | null
  status: ChallengeStatus
  createdAt: string
  respondedAt?: string | null
}

export interface SendChallengePayload {
  challengerTeamId: string
  challengedTeamId: string
  venueTeamId: string
  matchDate: string
  message?: string | null
  /**
   * Which of the challenger's own kits to wear. The backend still validates it and falls back
   * to its own automatic pick if it's missing or the team hasn't configured that kit.
   */
  challengerKitPreference?: TeamKitSlot | null
  /**
   * Which of the challenged team's kits it should wear, chosen by the challenger. The backend
   * still validates it and falls back to its own automatic, clash-avoiding pick if it's missing
   * or the team hasn't configured that kit.
   */
  challengedKitPreference?: TeamKitSlot | null
}
