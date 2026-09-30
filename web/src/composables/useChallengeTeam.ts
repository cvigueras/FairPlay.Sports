import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/lib/http'
import { CHALLENGE_ACTOR_ROLES, challengesApi } from '@/lib/challenges'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { SendChallengePayload } from '@/types/challenge'
import type { Team } from '@/types/team'

/**
 * The "challenge this team" flow shared by the team list and the team detail:
 * who may challenge, opening the wizard for a rival, and sending the challenge.
 * The screen renders one `ChallengeWizard` bound to `wizardOpen` / `rival`.
 */
export function useChallengeTeam() {
  const { t } = useI18n()
  const auth = useAuthStore()
  const ui = useUiStore()

  const wizardOpen = ref(false)
  const rival = ref<Team | null>(null)
  const sending = ref(false)

  /** Never your own team (any role), and only for users who can actually act for
   *  some team (a pure Player, with no Delegate/Coach/President/TechnicalStaff
   *  membership anywhere, has no team to send a challenge from). */
  function canChallenge(team: Team): boolean {
    if (auth.myTeams.some((membership) => membership.teamId === team.id)) return false
    return auth.myTeams.some((membership) => CHALLENGE_ACTOR_ROLES.includes(membership.role))
  }

  /** Members who can act for a team (not a plain Player) can mark it as open to challenges. */
  function canSetAccepts(team: Team): boolean {
    return auth.myTeams.some(
      (membership) => membership.teamId === team.id && CHALLENGE_ACTOR_ROLES.includes(membership.role),
    )
  }

  const togglingTeamId = ref<string | null>(null)

  /** Flips the team's "accepts challenges" flag and updates `team` in place. */
  async function toggleAccepts(team: Team) {
    togglingTeamId.value = team.id
    try {
      const updated = await teamsApi.setAcceptsChallenges(team.id, !team.acceptsChallenges, auth.accessToken)
      team.acceptsChallenges = updated.acceptsChallenges
      ui.notify(t(updated.acceptsChallenges ? 'teams.challengeStatus.enabled' : 'teams.challengeStatus.disabled'))
    } catch (err) {
      ui.notify(err instanceof ApiError ? err.message : t('teams.challengeStatus.failed'), 'error')
    } finally {
      togglingTeamId.value = null
    }
  }

  function open(team: Team) {
    rival.value = team
    wizardOpen.value = true
  }

  async function submit(payload: SendChallengePayload) {
    sending.value = true
    try {
      await challengesApi.send(payload, auth.accessToken)
      wizardOpen.value = false
      ui.notify(t('challenges.wizard.sentSuccess', { team: rival.value?.name }))
    } catch (err) {
      ui.notify(err instanceof ApiError ? err.message : t('challenges.wizard.sendFailed'), 'error')
    } finally {
      sending.value = false
    }
  }

  return { wizardOpen, rival, sending, canChallenge, open, submit, canSetAccepts, toggleAccepts, togglingTeamId }
}
