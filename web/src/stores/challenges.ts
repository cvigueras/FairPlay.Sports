import { ref } from 'vue'
import { defineStore } from 'pinia'
import { challengesApi, isActionablePending } from '@/lib/challenges'
import { useAuthStore } from '@/stores/auth'
import type { Challenge } from '@/types/challenge'
import type { TeamMemberRole } from '@/types/team'

/**
 * How many challenges the user can still accept or reject, shown as the nav badge next to
 * "Desafíos". AppShell fetches it once on app load; ChallengesView pushes an up-to-date count
 * straight from the data it already has (its own load, and after every accept/reject) so the
 * badge never needs a second fetch to stay in sync.
 */
export const useChallengesStore = defineStore('challenges', () => {
  const pendingCount = ref(0)

  function setPendingCount(count: number): void {
    pendingCount.value = count
  }

  async function refreshPendingCount(): Promise<void> {
    const auth = useAuthStore()
    await auth.loadMyTeams()
    const memberships = auth.myTeams
    const roleByTeamId: Record<string, TeamMemberRole> = Object.fromEntries(memberships.map((m) => [m.teamId, m.role]))

    const lists = await Promise.all(memberships.map((m) => challengesApi.forTeam(m.teamId, auth.accessToken)))
    const byId = new Map<string, Challenge>()
    for (const list of lists) for (const c of list) byId.set(c.id, c)

    pendingCount.value = [...byId.values()].filter((c) => isActionablePending(c, roleByTeamId)).length
  }

  return { pendingCount, setPendingCount, refreshPendingCount }
})
