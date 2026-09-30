import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { CreateTeamPayload, Team, TeamMemberRole } from '@/types/team'

/** What the team wizard hands back when the user finishes creating a team. */
export interface NewTeamInput {
  payload: CreateTeamPayload
  role: TeamMemberRole
  displayName: string
  crest: File | null
}

/**
 * The "create a new team" flow shared by My teams and the team list: the wizard's
 * open state, and creating the team (and its crest) and joining the creator to it.
 * The screen renders one `TeamWizard` bound to `wizardOpen` / `creating`.
 */
export function useCreateTeam() {
  const { t } = useI18n()
  const auth = useAuthStore()
  const ui = useUiStore()

  const wizardOpen = ref(false)
  const creating = ref(false)

  /** Creates the team, uploads its crest, joins the creator under the chosen role and
   *  closes the wizard. Returns the new team, or null when it failed (already notified). */
  async function create({ payload, role, displayName, crest }: NewTeamInput): Promise<Team | null> {
    creating.value = true
    try {
      const created = await teamsApi.create(payload, auth.accessToken)
      if (crest) await teamsApi.uploadCrest(created.id, crest, auth.accessToken)

      await auth.joinTeam(created.id, role, displayName)
      wizardOpen.value = false
      return { ...created, hasCrest: !!crest }
    } catch (error) {
      ui.notify(error instanceof ApiError ? error.message : t('profile.team.createFailed'), 'error')
      return null
    } finally {
      creating.value = false
    }
  }

  return { wizardOpen, creating, create }
}
