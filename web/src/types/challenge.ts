import type { KitPattern, PitchSurface } from '@/types/team'

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
  challengedTeamId: string
  challengedTeamName: string
  challengedTeamHasCrest: boolean
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
}
