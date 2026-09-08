import { baseUrl, http } from '@/lib/http'
import type { CreateTeamPayload, Team } from '@/types/team'

/** Thin client for the backend `TeamsController`. */
export const teamsApi = {
  list: (token?: string | null) => http.get<Team[]>('/api/Teams', { token }),

  create: (payload: CreateTeamPayload, token?: string | null) =>
    http.post<Team>('/api/Teams', payload, { token }),

  uploadCrest: (teamId: string, file: File, token?: string | null) => {
    const form = new FormData()
    form.append('file', file)
    return http.postForm<void>(`/api/Teams/${teamId}/crest`, form, { token })
  },

  /** Public endpoint that streams the crest image. */
  crestUrl: (teamId: string) => `${baseUrl}/api/Teams/${teamId}/crest`,
}
