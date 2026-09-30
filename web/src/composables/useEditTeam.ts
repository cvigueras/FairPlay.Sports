import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { CreateTeamPayload, Team } from '@/types/team'

/**
 * The "edit this team" flow shared by My teams and the team detail: which team the
 * wizard is editing, and saving the changes (and a new crest, if any).
 * The screen renders one `TeamWizard` with `:initial="editing"`.
 */
export function useEditTeam() {
  const { t } = useI18n()
  const auth = useAuthStore()
  const ui = useUiStore()

  /** The team being edited; the wizard is open exactly while this is set. */
  const editing = ref<Team | null>(null)
  const saving = ref(false)

  function open(team: Team) {
    editing.value = team
  }

  function close() {
    editing.value = null
  }

  /** Saves the wizard's changes and closes it. Returns the updated team, or null when it
   *  failed (already notified). */
  async function update({ payload, crest }: { payload: CreateTeamPayload; crest: File | null }): Promise<Team | null> {
    if (!editing.value) return null
    saving.value = true
    try {
      const updated = await teamsApi.update(editing.value.id, payload, auth.accessToken)
      if (crest) await teamsApi.uploadCrest(updated.id, crest, auth.accessToken)

      editing.value = null
      return crest ? { ...updated, hasCrest: true } : updated
    } catch (error) {
      ui.notify(error instanceof ApiError ? error.message : t('profile.team.updateFailed'), 'error')
      return null
    } finally {
      saving.value = false
    }
  }

  return { editing, saving, open, close, update }
}
