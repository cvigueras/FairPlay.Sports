import { ref } from 'vue'
import { defineStore } from 'pinia'

/** Where the router last sent someone before landing on a "detail" screen. */
export interface LastListRoute {
  name: string
  path: string
}

export type ToastColor = 'success' | 'error' | 'warning'

/**
 * Cross-page UI state that doesn't belong in a domain store. `breadcrumbLabel`
 * lets a view (e.g. the team detail page) supply the human-readable label for
 * a dynamic route segment (a team id) once it has loaded the data - the
 * breadcrumb itself is rendered by `AppShell`, which has no reason to know
 * how to fetch a team. `lastListRoute` is set by the router (see
 * `router/index.ts`) on every visit to a team-listing screen (Teams, My
 * teams, Standings), so a detail screen's breadcrumb can link back to
 * wherever the user actually came from instead of a hardcoded parent.
 *
 * `toast` backs the single app-wide snackbar (rendered in `App.vue`): any
 * view calls `notify` instead of rendering its own inline success/error
 * banner, so every transient result reads the same way (bottom-right,
 * auto-dismissed) no matter which screen triggered it.
 */
export const useUiStore = defineStore('ui', () => {
  const breadcrumbLabel = ref<string | null>(null)
  const lastListRoute = ref<LastListRoute | null>(null)

  const toastShow = ref(false)
  const toastMessage = ref('')
  const toastColor = ref<ToastColor>('success')

  function notify(message: string, color: ToastColor = 'success') {
    toastMessage.value = message
    toastColor.value = color
    toastShow.value = true
  }

  return { breadcrumbLabel, lastListRoute, toastShow, toastMessage, toastColor, notify }
})
