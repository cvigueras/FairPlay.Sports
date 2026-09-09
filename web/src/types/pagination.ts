/** Mirrors the backend `PagedResult<T>` (Application/Common/Querying). */
export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

/** Paging + sorting shared by every paged endpoint. */
export interface PageParams {
  page?: number
  pageSize?: number
  /** e.g. `name`, `-createdAt`, `city,-createdAt`. */
  sort?: string
}

/** Serialises page params plus arbitrary filters into a query string. */
export function toQueryString(params: Record<string, unknown>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, String(value))
    }
  }
  const query = search.toString()
  return query ? `?${query}` : ''
}
