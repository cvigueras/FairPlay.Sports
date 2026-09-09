import { baseUrl, http } from '@/lib/http'
import type { AgeCategory, CreateTeamPayload, Division, FootballType, Team } from '@/types/team'
import { toQueryString, type PageParams, type PagedResult } from '@/types/pagination'

export interface TeamFilters {
  name?: string
  city?: string
  type?: FootballType
  division?: Division
  category?: AgeCategory
  active?: boolean
}

export type TeamQuery = PageParams & TeamFilters

/** Thin client for the backend `TeamsController`. */
export const teamsApi = {
  /** One page of teams, with optional filters and sorting. */
  page: (query: TeamQuery = {}, token?: string | null) =>
    http.get<PagedResult<Team>>(`/api/Teams${toQueryString({ ...query })}`, { token }),

  /**
   * All teams as a flat list, for the (small) team picker. Capped at the
   * backend max page size - swap for a server-side autocomplete if the roster
   * grows past that.
   */
  list: (token?: string | null) =>
    teamsApi.page({ pageSize: 100, sort: 'name' }, token).then((result) => result.items),

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
