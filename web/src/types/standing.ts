/** Mirrors the backend `StandingDto`. */
export interface Standing {
  id: string
  teamId: string
  teamName: string
  teamHasCrest: boolean
  points: number
  played: number
  won: number
  drawn: number
  lost: number
  goalsFor: number
  goalsAgainst: number
  goalDifference: number
  createdAt: string
}
