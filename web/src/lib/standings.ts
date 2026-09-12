import { http } from '@/lib/http'
import type { Standing } from '@/types/standing'
import type { AgeCategory, Division, FootballType } from '@/types/team'
import { toQueryString, type PageParams, type PagedResult } from '@/types/pagination'

export interface StandingFilters {
  teamId?: string
  type?: FootballType
  division?: Division
  category?: AgeCategory
}

export type StandingQuery = PageParams & StandingFilters

/** Thin client for the backend `StandingsController`. */
export const standingsApi = {
  /** One page of the classification table, sorted (default: points, best first). */
  page: (query: StandingQuery = {}, token?: string | null) =>
    http.get<PagedResult<Standing>>(`/api/Standings${toQueryString({ ...query })}`, { token }),
}
