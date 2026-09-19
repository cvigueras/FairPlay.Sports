<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink } from 'vue-router'
import {
  mdiAccountGroupOutline,
  mdiCalendarOutline,
  mdiCheckCircleOutline,
  mdiCloseCircleOutline,
  mdiMapMarkerOutline,
  mdiPlusCircleOutline,
  mdiShieldOutline,
  mdiSwordCross,
} from '@mdi/js'
import TeamCrest from '@/components/TeamCrest.vue'
import { challengesApi } from '@/lib/challenges'
import { ApiError } from '@/lib/http'
import { MEMBER_ROLE_COLOR } from '@/lib/memberRole'
import { standingsApi } from '@/lib/standings'
import { teamsApi } from '@/lib/teams'
import { tonalStyle } from '@/lib/tonalColor'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { Challenge } from '@/types/challenge'
import type { Team, TeamMembership } from '@/types/team'

const { t, locale } = useI18n()
const auth = useAuthStore()
const ui = useUiStore()

const loading = ref(true)

interface TeamCard {
  membership: TeamMembership
  team: Team
}

interface TeamStandingSummary {
  position: number | null
  points: number
  played: number
  won: number
  drawn: number
  lost: number
}

const teamCards = ref<TeamCard[]>([])
const teamStandings = ref<Record<string, TeamStandingSummary>>({})
const challenges = ref<Challenge[]>([])

const myTeamIds = computed(() => new Set(teamCards.value.map((card) => card.team.id)))

/* ---- "Tus equipos" row: show only as many tiles as fit, no horizontal scroll ---- */

const TEAM_TILE_WIDTH = 148
const TEAM_TILE_GAP = 12

const teamCarouselEl = ref<HTMLElement | null>(null)
const visibleTeamSlotCount = ref(0)

const visibleTeamCards = computed(() => teamCards.value.slice(0, Math.min(visibleTeamSlotCount.value, teamCards.value.length)))
const showAddTeamTile = computed(() => visibleTeamSlotCount.value > teamCards.value.length)

function recomputeVisibleTeamSlots() {
  const width = teamCarouselEl.value?.clientWidth ?? 0
  const maxItems = Math.floor((width + TEAM_TILE_GAP) / (TEAM_TILE_WIDTH + TEAM_TILE_GAP))
  const combinedCount = teamCards.value.length + 1
  visibleTeamSlotCount.value = Math.max(0, Math.min(maxItems, combinedCount))
}

let teamCarouselResizeObserver: ResizeObserver | undefined

onBeforeUnmount(() => {
  teamCarouselResizeObserver?.disconnect()
})

watch(
  () => [teamCarouselEl.value, teamCards.value.length] as const,
  async ([el]) => {
    if (!el) return
    if (!teamCarouselResizeObserver) {
      teamCarouselResizeObserver = new ResizeObserver(() => recomputeVisibleTeamSlots())
    }
    teamCarouselResizeObserver.disconnect()
    teamCarouselResizeObserver.observe(el as HTMLElement)
    await nextTick()
    recomputeVisibleTeamSlots()
  },
)

/* ---- KPI row -------------------------------------------------------------- */

/** The distinct roles the user holds across their teams, e.g. "Presidente, Entrenador". */
const roleCaption = computed(() =>
  [...new Set(teamCards.value.map((card) => card.membership.role))]
    .map((role) => t(`profile.team.memberRoles.${role}`))
    .join(', '),
)

const pendingChallenges = computed(() => challenges.value.filter((c) => c.status === 'Pending'))
const sentPendingCount = computed(
  () => pendingChallenges.value.filter((c) => myTeamIds.value.has(c.challengerTeamId)).length,
)
const receivedPendingCount = computed(
  () => pendingChallenges.value.filter((c) => myTeamIds.value.has(c.challengedTeamId)).length,
)

/** Aggregated from each of the user's teams' current league standings - the app has no
 *  "match result" concept on a Challenge itself, only Pending/Accepted/Rejected, so a
 *  win/loss record is sourced from Standing.won/lost instead. */
const standingsTotals = computed(() => {
  const summaries = Object.values(teamStandings.value)
  return {
    won: summaries.reduce((sum, s) => sum + s.won, 0),
    lost: summaries.reduce((sum, s) => sum + s.lost, 0),
    drawn: summaries.reduce((sum, s) => sum + s.drawn, 0),
    played: summaries.reduce((sum, s) => sum + s.played, 0),
  }
})

const winRatePercent = computed(() => {
  const resolved = standingsTotals.value.won + standingsTotals.value.lost
  return resolved > 0 ? Math.round((standingsTotals.value.won / resolved) * 100) : null
})

const kpiCards = computed(() => [
  {
    key: 'teams',
    icon: mdiShieldOutline,
    color: '#16a34a',
    label: t('home.kpi.teams'),
    value: teamCards.value.length,
    caption: teamCards.value.length > 0 ? roleCaption.value : t('home.kpi.teamsCaptionEmpty'),
  },
  {
    key: 'active',
    icon: mdiSwordCross,
    color: '#1565c0',
    label: t('home.kpi.activeChallenges'),
    value: pendingChallenges.value.length,
    caption: t('home.kpi.activeChallengesCaption', {
      sent: sentPendingCount.value,
      received: receivedPendingCount.value,
    }),
  },
  {
    key: 'won',
    icon: mdiCheckCircleOutline,
    color: '#15803d',
    label: t('home.kpi.won'),
    value: standingsTotals.value.won,
    caption:
      winRatePercent.value !== null
        ? t('home.kpi.wonCaption', { pct: winRatePercent.value, played: standingsTotals.value.played })
        : t('home.kpi.wonCaptionEmpty'),
  },
  {
    key: 'lost',
    icon: mdiCloseCircleOutline,
    color: '#dc2626',
    label: t('home.kpi.lost'),
    value: standingsTotals.value.lost,
    caption: t('home.kpi.lostCaption'),
  },
])

/* ---- Hero: balance donut ---------------------------------------------------- */

const balanceDonutStyle = computed(() => {
  const { won, lost, drawn } = standingsTotals.value
  const total = won + lost + drawn
  if (total === 0) return { background: '#334155' }
  const wonDeg = (won / total) * 360
  const lostDeg = wonDeg + (lost / total) * 360
  return {
    background: `conic-gradient(#4ade80 0deg ${wonDeg}deg, #fb7185 ${wonDeg}deg ${lostDeg}deg, #64748b ${lostDeg}deg 360deg)`,
  }
})

/* ---- Next match -------------------------------------------------------------- */

const nextMatch = computed(() => {
  const now = Date.now()
  return (
    challenges.value
      .filter((c) => c.status === 'Accepted' && new Date(c.matchDate).getTime() > now)
      .sort((a, b) => new Date(a.matchDate).getTime() - new Date(b.matchDate).getTime())[0] ?? null
  )
})

const nextMatchOpponentName = computed(() => {
  if (!nextMatch.value) return ''
  return myTeamIds.value.has(nextMatch.value.challengerTeamId)
    ? nextMatch.value.challengedTeamName
    : nextMatch.value.challengerTeamName
})

const nextMatchDate = computed(() => (nextMatch.value ? new Date(nextMatch.value.matchDate) : null))

/* ---- Standings mini table ----------------------------------------------------- */

const rankedTeamCards = computed(() =>
  teamCards.value
    .map((card) => ({ ...card, standing: teamStandings.value[card.team.id] }))
    .filter((card): card is TeamCard & { standing: TeamStandingSummary } => !!card.standing)
    .sort((a, b) => b.standing.points - a.standing.points),
)

/* ---- Recent activity, derived from real, timestamped events -------------------- */

interface ActivityEntry {
  id: string
  icon: string
  color: string
  text: string
  at: Date
}

function relativeTime(date: Date): string {
  const diffMs = date.getTime() - Date.now()
  const diffHours = Math.round(diffMs / 3_600_000)
  const diffDays = Math.round(diffMs / 86_400_000)
  const rtf = new Intl.RelativeTimeFormat(locale.value, { numeric: 'auto' })
  if (Math.abs(diffHours) < 24) return rtf.format(diffHours, 'hour')
  if (Math.abs(diffDays) < 30) return rtf.format(diffDays, 'day')
  return rtf.format(Math.round(diffDays / 30), 'month')
}

const activityEntries = computed<ActivityEntry[]>(() => {
  const entries: ActivityEntry[] = []

  for (const { membership, team } of teamCards.value) {
    entries.push({
      id: `join-${membership.id}`,
      icon: mdiAccountGroupOutline,
      color: '#1565c0',
      text: t('home.activity.joinedTeam', {
        team: team.name,
        role: t(`profile.team.memberRoles.${membership.role}`),
      }),
      at: new Date(membership.createdAt),
    })
  }

  for (const challenge of challenges.value) {
    const iAmChallenger = myTeamIds.value.has(challenge.challengerTeamId)
    const iAmChallenged = myTeamIds.value.has(challenge.challengedTeamId)
    const otherName = iAmChallenger ? challenge.challengedTeamName : challenge.challengerTeamName

    if (challenge.status === 'Pending') {
      entries.push({
        id: `challenge-${challenge.id}`,
        icon: mdiSwordCross,
        color: '#1565c0',
        text: iAmChallenger
          ? t('home.activity.challengeSent', { team: otherName })
          : t('home.activity.challengeReceived', { team: otherName }),
        at: new Date(challenge.createdAt),
      })
    } else if (challenge.respondedAt) {
      const key =
        challenge.status === 'Accepted'
          ? iAmChallenged
            ? 'challengeAcceptedByMe'
            : 'challengeAcceptedByOther'
          : iAmChallenged
            ? 'challengeRejectedByMe'
            : 'challengeRejectedByOther'
      entries.push({
        id: `challenge-${challenge.id}`,
        icon: challenge.status === 'Accepted' ? mdiCheckCircleOutline : mdiCloseCircleOutline,
        color: challenge.status === 'Accepted' ? '#15803d' : '#dc2626',
        text: t(`home.activity.${key}`, { team: otherName }),
        at: new Date(challenge.respondedAt),
      })
    }
  }

  return entries.sort((a, b) => b.at.getTime() - a.at.getTime()).slice(0, 5)
})

/* ---- Data loading -------------------------------------------------------------- */

async function loadStandingsFor(team: Team): Promise<void> {
  const result = await standingsApi.page(
    { type: team.type, division: team.division ?? undefined, category: team.category, pageSize: 100 },
    auth.accessToken,
  )
  const standing = result.items.find((s) => s.teamId === team.id)
  if (!standing) return
  teamStandings.value = {
    ...teamStandings.value,
    [team.id]: {
      position: result.items.indexOf(standing) + 1,
      points: standing.points,
      played: standing.played,
      won: standing.won,
      drawn: standing.drawn,
      lost: standing.lost,
    },
  }
}

onMounted(async () => {
  try {
    await auth.loadMyTeams()
    const memberships = auth.myTeams
    const teams = await Promise.all(memberships.map((m) => teamsApi.byId(m.teamId, auth.accessToken)))
    const teamById = new Map(teams.map((team) => [team.id, team]))
    teamCards.value = memberships
      .map((membership) => ({ membership, team: teamById.get(membership.teamId) }))
      .filter((card): card is TeamCard => !!card.team)

    const [challengeLists] = await Promise.all([
      Promise.all(teams.map((team) => challengesApi.forTeam(team.id, auth.accessToken))),
      Promise.all(teams.map((team) => loadStandingsFor(team))),
    ])
    const byId = new Map<string, Challenge>()
    for (const list of challengeLists) for (const challenge of list) byId.set(challenge.id, challenge)
    challenges.value = [...byId.values()]
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('home.loadFailed'), 'error')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <v-main>
    <v-container class="py-6 py-md-10 home-container">
      <v-progress-circular v-if="loading" indeterminate color="primary" class="d-block mx-auto my-16" />

      <template v-else>
        <!-- Header -->
        <div class="home-header mb-7">
          <div class="d-flex ga-2 home-header-actions">
            <RouterLink :to="{ name: 'my-teams' }" class="fp-btn fp-btn-outline">
              {{ t('home.viewMyTeams') }}
            </RouterLink>
            <RouterLink :to="{ name: 'teams' }" class="fp-btn fp-btn-solid">
              {{ t('home.browseTeams') }}
            </RouterLink>
          </div>
        </div>

        <!-- KPI row -->
        <div class="home-kpi-grid mb-6">
          <v-card v-for="kpi in kpiCards" :key="kpi.key" border flat rounded="xl" class="pa-5 home-kpi-card">
            <div class="d-flex align-center justify-space-between mb-2">
              <span class="home-kpi-label">{{ kpi.label }}</span>
              <div class="home-kpi-icon" :style="tonalStyle(kpi.color)">
                <v-icon :icon="kpi.icon" size="17" :color="kpi.color" />
              </div>
            </div>
            <div class="home-kpi-value">{{ kpi.value }}</div>
            <span class="home-kpi-caption">{{ kpi.caption }}</span>
          </v-card>
        </div>

        <!-- Hero: balance -->
        <v-card rounded="xl" class="home-hero mb-6 pa-6 pa-md-8">
          <div class="home-hero-inner">
            <div class="home-donut-wrap">
              <div class="home-donut" :style="balanceDonutStyle" />
              <div class="home-donut-hole">
                <template v-if="winRatePercent !== null">
                  <span class="home-donut-pct">{{ winRatePercent }}%</span>
                  <span class="home-donut-caption">{{ t('home.balance.winRate') }}</span>
                </template>
                <span v-else class="home-donut-empty">{{ t('home.balance.empty') }}</span>
              </div>
            </div>

            <div class="home-hero-legend">
              <span class="home-hero-legend-title">{{ t('home.balance.title') }}</span>
              <div class="home-hero-legend-row">
                <span class="home-hero-dot" style="background: #4ade80" />
                <span class="home-hero-legend-label">{{ t('home.balance.won') }}</span>
                <span class="home-hero-legend-value">{{ standingsTotals.won }}</span>
              </div>
              <div class="home-hero-legend-row">
                <span class="home-hero-dot" style="background: #fb7185" />
                <span class="home-hero-legend-label">{{ t('home.balance.lost') }}</span>
                <span class="home-hero-legend-value">{{ standingsTotals.lost }}</span>
              </div>
              <div class="home-hero-legend-row">
                <span class="home-hero-dot" style="background: #64748b" />
                <span class="home-hero-legend-label">{{ t('home.balance.drawn') }}</span>
                <span class="home-hero-legend-value">{{ standingsTotals.drawn }}</span>
              </div>
            </div>

            <div class="home-hero-divider" />

            <div class="home-hero-stats">
              <div class="home-hero-stat">
                <div class="home-hero-stat-icon">
                  <v-icon :icon="mdiShieldOutline" size="19" color="#e2e8f0" />
                </div>
                <div>
                  <div class="home-hero-stat-value">{{ teamCards.length }}</div>
                  <div class="home-hero-stat-label">{{ t('home.stats.teams') }}</div>
                </div>
              </div>
              <div class="home-hero-stat">
                <div class="home-hero-stat-icon">
                  <v-icon :icon="mdiSwordCross" size="19" color="#e2e8f0" />
                </div>
                <div>
                  <div class="home-hero-stat-value">{{ challenges.length }}</div>
                  <div class="home-hero-stat-label">{{ t('home.stats.challenges') }}</div>
                </div>
              </div>
              <div class="home-hero-stat">
                <div class="home-hero-stat-icon">
                  <v-icon :icon="mdiCheckCircleOutline" size="19" color="#e2e8f0" />
                </div>
                <div>
                  <div class="home-hero-stat-value">{{ standingsTotals.played }}</div>
                  <div class="home-hero-stat-label">{{ t('home.stats.played') }}</div>
                </div>
              </div>
            </div>
          </div>
        </v-card>

        <!-- Row B: teams + next match -->
        <div class="home-row-b mb-6">
          <v-card border flat rounded="xl" class="pa-5">
            <div class="d-flex align-center justify-space-between mb-3">
              <h2 class="text-subtitle-1 font-weight-bold">{{ t('home.myTeams.title') }}</h2>
              <RouterLink :to="{ name: 'my-teams' }" class="home-link">{{ t('home.myTeams.viewAll') }}</RouterLink>
            </div>

            <div v-if="teamCards.length > 0" ref="teamCarouselEl" class="home-team-carousel">
              <RouterLink
                v-for="{ membership, team } in visibleTeamCards"
                :key="membership.id"
                :to="{ name: 'team-detail', params: { id: team.id } }"
                class="home-team-tile"
              >
                <TeamCrest :team="team" :size="36" />
                <span class="home-team-tile-name">{{ team.name }}</span>
                <span class="home-team-tile-role" :style="tonalStyle(MEMBER_ROLE_COLOR[membership.role])">
                  {{ t(`profile.team.memberRoles.${membership.role}`) }}
                </span>
              </RouterLink>
              <RouterLink v-if="showAddTeamTile" :to="{ name: 'my-teams' }" class="home-team-tile home-team-tile--add">
                <v-icon :icon="mdiPlusCircleOutline" size="20" />
                {{ t('home.myTeams.createAnother') }}
              </RouterLink>
            </div>
            <p v-else class="text-body-2 text-medium-emphasis">{{ t('home.myTeams.empty') }}</p>
          </v-card>

          <v-card border flat rounded="xl" class="pa-5 d-flex ga-4 align-center home-next-match">
            <template v-if="nextMatch && nextMatchDate">
              <div class="home-next-match-date">
                <span class="home-next-match-month">
                  {{ nextMatchDate.toLocaleDateString(locale, { month: 'short' }) }}
                </span>
                <span class="home-next-match-day">{{ nextMatchDate.getDate() }}</span>
              </div>
              <div class="min-width-0">
                <span class="home-kpi-label">{{ t('home.nextMatch.title') }}</span>
                <div class="home-next-match-opponent">vs. {{ nextMatchOpponentName }}</div>
                <div class="home-next-match-meta">
                  <v-icon :icon="mdiCalendarOutline" size="14" />
                  {{ nextMatchDate.toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit' }) }}
                  <template v-if="nextMatch.venueName">
                    · <v-icon :icon="mdiMapMarkerOutline" size="14" /> {{ nextMatch.venueName }}
                  </template>
                </div>
              </div>
            </template>
            <div v-else class="home-empty-block">
              <span class="text-body-2 text-medium-emphasis">{{ t('home.nextMatch.empty') }}</span>
              <RouterLink :to="{ name: 'teams' }" class="home-link">{{ t('home.nextMatch.emptyCta') }}</RouterLink>
            </div>
          </v-card>
        </div>

        <!-- Row C: standings + activity -->
        <div class="home-row-c">
          <v-card border flat rounded="xl" class="pa-5">
            <div class="d-flex align-center justify-space-between mb-3">
              <h2 class="text-subtitle-1 font-weight-bold">{{ t('home.standings.title') }}</h2>
              <RouterLink :to="{ name: 'standings' }" class="home-link">{{ t('home.standings.viewAll') }}</RouterLink>
            </div>

            <template v-if="rankedTeamCards.length > 0">
              <div class="home-standings-row home-standings-head">
                <span>{{ t('home.standings.position') }}</span>
                <span>{{ t('home.standings.team') }}</span>
                <span class="text-right">{{ t('home.standings.played') }}</span>
                <span class="text-right">{{ t('home.standings.points') }}</span>
              </div>
              <div
                v-for="(card, index) in rankedTeamCards"
                :key="card.team.id"
                class="home-standings-row"
                :class="{ 'home-standings-row--highlight': index === 0 }"
              >
                <span class="home-standings-pos">{{ card.standing.position }}.º</span>
                <span class="home-standings-team">{{ card.team.name }}</span>
                <span class="text-right home-standings-muted">{{ card.standing.played }}</span>
                <span class="text-right home-standings-points">{{ card.standing.points }}</span>
              </div>
            </template>
            <p v-else class="text-body-2 text-medium-emphasis">{{ t('home.standings.empty') }}</p>
          </v-card>

          <v-card border flat rounded="xl" class="pa-5">
            <h2 class="text-subtitle-1 font-weight-bold mb-3">{{ t('home.activity.title') }}</h2>

            <template v-if="activityEntries.length > 0">
              <div v-for="entry in activityEntries" :key="entry.id" class="home-activity-row">
                <div class="home-activity-icon" :style="tonalStyle(entry.color)">
                  <v-icon :icon="entry.icon" size="15" :color="entry.color" />
                </div>
                <div>
                  <div class="home-activity-text">{{ entry.text }}</div>
                  <div class="home-activity-time">{{ relativeTime(entry.at) }}</div>
                </div>
              </div>
            </template>
            <p v-else class="text-body-2 text-medium-emphasis">{{ t('home.activity.empty') }}</p>
          </v-card>
        </div>
      </template>
    </v-container>
  </v-main>
</template>

<style scoped>
.home-container {
  max-width: 1200px;
}

.home-header {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 16px;
}

.home-kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.home-kpi-label {
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.home-kpi-icon {
  width: 30px;
  height: 30px;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.home-kpi-value {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 30px;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
}

.home-kpi-caption {
  font-size: 12.5px;
  color: #64748b;
}

.home-hero {
  background: #1e293b !important;
  color: #fff;
}

.home-hero-inner {
  display: flex;
  align-items: center;
  gap: 32px;
  flex-wrap: wrap;
}

.home-donut-wrap {
  position: relative;
  width: 140px;
  height: 140px;
  flex-shrink: 0;
}

.home-donut {
  width: 140px;
  height: 140px;
  border-radius: 999px;
}

.home-donut-hole {
  position: absolute;
  top: 13px;
  left: 13px;
  width: 114px;
  height: 114px;
  border-radius: 999px;
  background: #1e293b;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 8px;
}

.home-donut-pct {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 26px;
  font-weight: 700;
}

.home-donut-caption {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 2px;
}

.home-donut-empty {
  font-size: 12px;
  color: #94a3b8;
}

.home-hero-legend {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.home-hero-legend-title {
  font-size: 11.5px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.home-hero-legend-row {
  display: flex;
  align-items: center;
  gap: 9px;
  font-size: 14px;
}

.home-hero-dot {
  width: 9px;
  height: 9px;
  border-radius: 999px;
  flex-shrink: 0;
}

.home-hero-legend-value {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  margin-left: auto;
  padding-left: 12px;
}

.home-hero-divider {
  width: 1px;
  align-self: stretch;
  background: #334155;
}

.home-hero-stats {
  display: flex;
  flex-direction: column;
  gap: 14px;
  flex-grow: 1;
  min-width: 200px;
}

.home-hero-stat {
  display: flex;
  align-items: center;
  gap: 12px;
}

.home-hero-stat-icon {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.home-hero-stat-value {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 17px;
  font-weight: 700;
}

.home-hero-stat-label {
  font-size: 12px;
  color: #94a3b8;
}

.home-row-b {
  display: grid;
  grid-template-columns: 1.5fr 1fr;
  gap: 16px;
  align-items: stretch;
}

.home-row-c {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.home-link {
  font-size: 13px;
  font-weight: 600;
}

.home-team-carousel {
  display: flex;
  gap: 12px;
  flex-wrap: nowrap;
  overflow: hidden;
}

.home-team-tile {
  width: 148px;
  flex-shrink: 0;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 8px;
  color: inherit;
  text-decoration: none;
}

.home-team-tile:hover {
  border-color: rgb(var(--v-theme-primary));
}

.home-team-tile-name {
  font-size: 13px;
  font-weight: 700;
  color: #0f172a;
}

.home-team-tile-role {
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 10.5px;
  font-weight: 700;
}

.home-team-tile--add {
  align-items: center;
  justify-content: center;
  flex-direction: row;
  gap: 6px;
  border-style: dashed;
  color: #94a3b8;
  font-size: 12.5px;
  font-weight: 600;
  text-align: center;
}

.home-next-match {
  min-height: 96px;
}

.home-next-match-date {
  width: 58px;
  flex-shrink: 0;
  border-radius: 12px;
  background: rgba(var(--v-theme-primary), 0.1);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 10px 0;
}

.home-next-match-month {
  font-size: 10.5px;
  font-weight: 700;
  color: rgb(var(--v-theme-primary));
  text-transform: uppercase;
}

.home-next-match-day {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 22px;
  font-weight: 700;
  color: rgb(var(--v-theme-primary));
}

.home-next-match-opponent {
  font-size: 14.5px;
  font-weight: 700;
  color: #0f172a;
  margin-top: 4px;
}

.home-next-match-meta {
  font-size: 12.5px;
  color: #64748b;
  margin-top: 3px;
  display: flex;
  align-items: center;
  gap: 4px;
  flex-wrap: wrap;
}

.home-empty-block {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.home-standings-row {
  display: grid;
  grid-template-columns: 40px 1fr 48px 48px;
  gap: 8px;
  align-items: center;
  padding: 9px 6px;
  border-radius: 10px;
}

.home-standings-head {
  font-size: 11px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  padding-top: 0;
  padding-bottom: 8px;
}

.home-standings-row--highlight {
  background: rgba(var(--v-theme-primary), 0.07);
}

.home-standings-pos {
  font-weight: 700;
  color: #64748b;
}

.home-standings-row--highlight .home-standings-pos {
  color: rgb(var(--v-theme-primary));
}

.home-standings-team {
  font-size: 13.5px;
  font-weight: 600;
  color: #334155;
}

.home-standings-muted {
  font-size: 13px;
  color: #64748b;
}

.home-standings-points {
  font-size: 13.5px;
  font-weight: 700;
  color: #0f172a;
}

.home-activity-row {
  display: flex;
  gap: 12px;
  padding-bottom: 14px;
}

.home-activity-row:last-child {
  padding-bottom: 0;
}

.home-activity-icon {
  width: 30px;
  height: 30px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.home-activity-text {
  font-size: 13.5px;
  color: #0f172a;
}

.home-activity-time {
  font-size: 12px;
  color: #94a3b8;
  margin-top: 2px;
}

@media (max-width: 960px) {
  .home-kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 899px) {
  .home-row-b,
  .home-row-c {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 599px) {
  .home-kpi-grid {
    grid-template-columns: 1fr;
  }

  .home-header-actions {
    width: 100%;
  }

  .home-header-actions .fp-btn {
    flex: 1;
    text-align: center;
  }
}
</style>
