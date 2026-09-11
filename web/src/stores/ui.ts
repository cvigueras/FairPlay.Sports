import { ref } from 'vue'
import { defineStore } from 'pinia'

/**
 * Cross-page UI state that doesn't belong in a domain store. `breadcrumbLabel`
 * lets a view (e.g. the team detail page) supply the human-readable label for
 * a dynamic route segment (a team id) once it has loaded the data - the
 * breadcrumb itself is rendered by `AppShell`, which has no reason to know
 * how to fetch a team.
 */
export const useUiStore = defineStore('ui', () => {
  const breadcrumbLabel = ref<string | null>(null)

  return { breadcrumbLabel }
})
