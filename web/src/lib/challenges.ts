import { http } from '@/lib/http'
import type { Challenge, SendChallengePayload } from '@/types/challenge'
import type { TeamMemberRole } from '@/types/team'

/** Who may send or respond to a challenge on a team's behalf - mirrors the backend's
 *  `ChallengeAuthorization.CanActForTeam`. */
export const CHALLENGE_ACTOR_ROLES: TeamMemberRole[] = ['Delegate', 'Coach', 'President', 'TechnicalStaff']

/** Thin client for the backend `ChallengesController`. */
export const challengesApi = {
  /** Every challenge a team sent or received. */
  forTeam: (teamId: string, token?: string | null) =>
    http.get<Challenge[]>(`/api/Challenges?teamId=${teamId}`, { token }),

  send: (payload: SendChallengePayload, token?: string | null) =>
    http.post<Challenge>('/api/Challenges', payload, { token }),

  accept: (id: string, token?: string | null) =>
    http.post<Challenge>(`/api/Challenges/${id}/accept`, undefined, { token }),

  reject: (id: string, token?: string | null) =>
    http.post<Challenge>(`/api/Challenges/${id}/reject`, undefined, { token }),
}
