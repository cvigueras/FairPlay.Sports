import { ref } from 'vue'
import { defineStore } from 'pinia'

/** Where the router last sent someone before landing on a "detail" screen. */
export interface LastListRoute {
  name: string
  path: string
}

/**
 * Cross-page UI state that doesn't belong in a domain store. `breadcrumbLabel`
 * lets a view (e.g. the team detail page) supply the human-readable label for
 * a dynamic route segment (a team id) once it has loaded the data - the
 * breadcrumb itself is rendered by `AppShell`, which has no reason to know
 * how to fetch a team. `lastListRoute` is set by the router (see
 * `router/index.ts`) on every visit to a team-listing screen (Teams, My
 * teams, Standings), so a detail screen's breadcrumb can link back to
 * wherever the user actually came from instead of a hardcoded parent.
 */
export const useUiStore = defineStore('ui', () => {
  const breadcrumbLabel = ref<string | null>(null)
  const lastListRoute = ref<LastListRoute | null>(null)

  return { breadcrumbLabel, lastListRoute }
})
