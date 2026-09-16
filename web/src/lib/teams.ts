import { baseUrl, http } from '@/lib/http'
import type {
  AgeCategory,
  CreateTeamPayload,
  Division,
  FootballType,
  Team,
  TeamMemberRole,
  TeamMembership,
  UpdateTeamPayload,
} from '@/types/team'
import { toQueryString, type PageParams, type PagedResult } from '@/types/pagination'

export interface TeamFilters {
  name?: string
  coach?: string
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

  /** A single team by id (used to resolve the user's own team). */
  byId: (id: string, token?: string | null) =>
    http.get<Team>(`/api/Teams/${id}`, { token }),

  /**
   * All teams as a flat list, for the (small) team picker. Capped at the
   * backend max page size - swap for a server-side autocomplete if the roster
   * grows past that.
   */
  list: (token?: string | null) =>
    teamsApi.page({ pageSize: 100, sort: 'name' }, token).then((result) => result.items),

  create: (payload: CreateTeamPayload, token?: string | null) =>
    http.post<Team>('/api/Teams', payload, { token }),

  update: (id: string, payload: UpdateTeamPayload, token?: string | null) =>
    http.put<Team>(`/api/Teams/${id}`, payload, { token }),

  uploadCrest: (teamId: string, file: File, token?: string | null) => {
    const form = new FormData()
    form.append('file', file)
    return http.postForm<void>(`/api/Teams/${teamId}/crest`, form, { token })
  },

  /** Public endpoint that streams the crest image. */
  crestUrl: (teamId: string) => `${baseUrl}/api/Teams/${teamId}/crest`,

  members: {
    /** All members of a team, with their role and in-team display name. */
    list: (teamId: string, token?: string | null) =>
      http.get<TeamMembership[]>(`/api/Teams/${teamId}/members`, { token }),

    /** Adds a user to the team with a given role - founding it or joining an existing one. */
    join: (
      teamId: string,
      payload: { userId: string; role: TeamMemberRole; displayName: string },
      token?: string | null,
    ) => http.post<TeamMembership>(`/api/Teams/${teamId}/members`, payload, { token }),

    /** Removes a user from the team. */
    leave: (teamId: string, userId: string, token?: string | null) =>
      http.del<void>(`/api/Teams/${teamId}/members/${userId}`, { token }),
  },
}
