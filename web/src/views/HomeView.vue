<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink } from 'vue-router'
import {
  mdiAccountGroupOutline,
  mdiCalendarBlankOutline,
  mdiCalendarOutline,
  mdiChevronRight,
  mdiCheckCircleOutline,
  mdiCloseCircleOutline,
  mdiMagnify,
  mdiMapMarkerOutline,
  mdiPlusOutline,
  mdiShieldOutline,
  mdiSwordCross,
} from '@mdi/js'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { challengesApi } from '@/lib/challenges'
import { DIVISION_COLOR } from '@/lib/division'
import { ApiError } from '@/lib/http'
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

/* ---- Row B: match the "standings" + "teams" panels' height to the hero row above ---- */

const ROW_B_EXTRA_HEIGHT = 80

const heroRowEl = ref<HTMLElement | null>(null)
const heroRowHeight = ref<number | null>(null)
const desktopRowMql = window.matchMedia('(min-width: 900px)')

const ROW_B_HEIGHT_SCALE = 0.85

const rowBCardStyle = computed(() =>
  heroRowHeight.value
    ? { height: `${(heroRowHeight.value + ROW_B_EXTRA_HEIGHT) * ROW_B_HEIGHT_SCALE}px` }
    : undefined,
)

function recomputeHeroRowHeight() {
  heroRowHeight.value = desktopRowMql.matches ? (heroRowEl.value?.clientHeight ?? null) : null
}

let heroRowResizeObserver: ResizeObserver | undefined

onBeforeUnmount(() => {
  heroRowResizeObserver?.disconnect()
  kpiRowResizeObserver?.disconnect()
  desktopRowMql.removeEventListener('change', recomputeHeroRowHeight)
  desktopRowMql.removeEventListener('change', recomputeKpiRowHeight)
})

desktopRowMql.addEventListener('change', recomputeHeroRowHeight)
desktopRowMql.addEventListener('change', recomputeKpiRowHeight)

watch(
  () => heroRowEl.value,
  async (el) => {
    if (!el) return
    if (!heroRowResizeObserver) {
      heroRowResizeObserver = new ResizeObserver(() => recomputeHeroRowHeight())
    }
    heroRowResizeObserver.disconnect()
    heroRowResizeObserver.observe(el)
    await nextTick()
    recomputeHeroRowHeight()
  },
)

/* ---- Sidebar: stretch the activity panel flush with the row-b cards below.
   Derived from the same measured heights as rowBCardStyle (plus the KPI
   row), rather than re-measuring the main column directly - the main
   column's own box size changes in lockstep with heroRowHeight/rowBCardStyle
   one render later, so observing it directly races those updates. ---- */

const ROW_GAP = 24

const kpiRowEl = ref<HTMLElement | null>(null)
const kpiRowHeight = ref<number | null>(null)

const activityPanelStyle = computed(() => {
  if (!kpiRowHeight.value || !heroRowHeight.value) return undefined
  const rowBHeight = (heroRowHeight.value + ROW_B_EXTRA_HEIGHT) * ROW_B_HEIGHT_SCALE
  return { height: `${kpiRowHeight.value + ROW_GAP + heroRowHeight.value + ROW_GAP + rowBHeight}px` }
})

function recomputeKpiRowHeight() {
  kpiRowHeight.value = desktopRowMql.matches ? (kpiRowEl.value?.clientHeight ?? null) : null
}

let kpiRowResizeObserver: ResizeObserver | undefined

watch(
  () => kpiRowEl.value,
  async (el) => {
    if (!el) return
    if (!kpiRowResizeObserver) {
      kpiRowResizeObserver = new ResizeObserver(() => recomputeKpiRowHeight())
    }
    kpiRowResizeObserver.disconnect()
    kpiRowResizeObserver.observe(el)
    await nextTick()
    recomputeKpiRowHeight()
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

/* ---- Upcoming matches ---------------------------------------------------------- */

interface UpcomingMatchCard {
  id: string
  opponent: string
  date: Date
  venueName?: string | null
}

const UPCOMING_MATCHES_VISIBLE = 3

const upcomingMatches = computed<UpcomingMatchCard[]>(() => {
  const now = Date.now()
  return challenges.value
    .filter((c) => c.status === 'Accepted' && new Date(c.matchDate).getTime() > now)
    .map((c) => ({
      id: c.id,
      opponent: myTeamIds.value.has(c.challengerTeamId) ? c.challengedTeamName : c.challengerTeamName,
      date: new Date(c.matchDate),
      venueName: c.venueName,
    }))
    .sort((a, b) => a.date.getTime() - b.date.getTime())
})

const visibleUpcomingMatches = computed(() => upcomingMatches.value.slice(0, UPCOMING_MATCHES_VISIBLE))
const upcomingMatchesOverflowCount = computed(() =>
  Math.max(0, upcomingMatches.value.length - UPCOMING_MATCHES_VISIBLE),
)

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

  return entries.sort((a, b) => b.at.getTime() - a.at.getTime())
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
    <v-container class="py-6 py-md-10 home-outer-container">
      <v-progress-circular v-if="loading" indeterminate color="primary" class="d-block mx-auto my-16" />

      <template v-else>
      <div class="home-layout">
        <div class="home-main-col">
        <!-- KPI row -->
        <div ref="kpiRowEl" class="home-kpi-grid mb-6">
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

        <!-- Hero: balance + next match -->
        <div ref="heroRowEl" class="home-hero-row mb-6">
          <v-card rounded="xl" class="home-hero pa-6 pa-md-8">
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

          <v-card border flat rounded="xl" class="pa-5 home-next-match">
            <div class="home-next-match-header">
              <h2>{{ t('home.nextMatch.title') }}</h2>
              <span v-if="upcomingMatches.length > 0" class="home-next-match-count" :style="tonalStyle('#16a34a')">
                {{ upcomingMatches.length }}
              </span>
            </div>

            <template v-if="upcomingMatches.length > 0">
              <div class="home-next-match-list">
                <RouterLink
                  v-for="(match, index) in visibleUpcomingMatches"
                  :key="match.id"
                  :to="{ name: 'challenges' }"
                  class="home-next-match-row"
                  :class="{ 'home-next-match-row--next': index === 0 }"
                >
                  <div class="home-next-match-date" :class="{ 'home-next-match-date--next': index === 0 }">
                    <span class="home-next-match-month">
                      {{ match.date.toLocaleDateString(locale, { month: 'short' }) }}
                    </span>
                    <span class="home-next-match-day">{{ match.date.getDate() }}</span>
                  </div>
                  <div class="min-width-0">
                    <div v-if="index === 0" class="home-next-match-tag">{{ t('home.nextMatch.next') }}</div>
                    <div class="home-next-match-opponent">vs. {{ match.opponent }}</div>
                    <div class="home-next-match-meta">
                      <v-icon :icon="mdiCalendarOutline" size="12" />
                      {{ match.date.toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit' }) }}
                      <template v-if="match.venueName">
                        · <v-icon :icon="mdiMapMarkerOutline" size="12" />
                        <span class="home-next-match-venue">{{ match.venueName }}</span>
                      </template>
                    </div>
                  </div>
                </RouterLink>
              </div>
              <RouterLink
                v-if="upcomingMatchesOverflowCount > 0"
                :to="{ name: 'challenges' }"
                class="home-next-match-overflow"
              >
                {{ t('home.nextMatch.more', { count: upcomingMatchesOverflowCount }) }}
                <v-icon :icon="mdiChevronRight" size="14" />
              </RouterLink>
            </template>
            <div v-else class="home-empty-block">
              <div class="home-empty-icon">
                <v-icon :icon="mdiCalendarBlankOutline" size="18" color="#94a3b8" />
              </div>
              <span class="text-body-2 text-medium-emphasis">{{ t('home.nextMatch.empty') }}</span>
              <RouterLink :to="{ name: 'teams' }" class="home-next-match-cta">
                <v-icon :icon="mdiMagnify" size="14" />
                {{ t('home.browseTeams') }}
              </RouterLink>
            </div>
          </v-card>
        </div>

        <!-- Row B: teams + standings -->
        <div class="home-row-b">
          <v-card border flat rounded="xl" class="pa-5 home-row-b-card home-teams-panel" :style="rowBCardStyle">
            <div class="home-teams-header">
              <h2>{{ t('home.myTeams.title') }}</h2>
              <span v-if="teamCards.length > 0" class="home-teams-count" :style="tonalStyle('#16a34a')">
                {{ teamCards.length }}
              </span>
            </div>

            <template v-if="teamCards.length > 0">
              <div class="home-teams-list home-activity-scroll">
                <RouterLink
                  v-for="{ membership, team } in teamCards"
                  :key="membership.id"
                  :to="{ name: 'team-detail', params: { id: team.id } }"
                  class="home-teams-row"
                >
                  <TeamCrest :team="team" :size="34" />
                  <div class="min-width-0">
                    <div class="home-teams-row-name">{{ team.name }}</div>
                    <div class="home-teams-row-tags">
                      <span class="home-teams-tag" :style="tonalStyle(AGE_CATEGORY_COLOR[team.category])">
                        {{ t(`profile.team.enums.${team.category}`) }}
                      </span>
                      <span v-if="team.division" class="home-teams-tag" :style="tonalStyle(DIVISION_COLOR[team.division])">
                        {{ t(`profile.team.enums.${team.division}`) }}
                      </span>
                    </div>
                  </div>
                </RouterLink>
              </div>
              <RouterLink :to="{ name: 'my-teams' }" class="home-teams-add">
                <v-icon :icon="mdiPlusOutline" size="14" />
                {{ t('home.myTeams.createAnother') }}
              </RouterLink>
            </template>
            <div v-else class="home-empty-block">
              <div class="home-empty-icon">
                <v-icon :icon="mdiShieldOutline" size="18" color="#94a3b8" />
              </div>
              <span class="text-body-2 text-medium-emphasis">{{ t('home.myTeams.empty') }}</span>
              <RouterLink :to="{ name: 'teams' }" class="fp-btn fp-btn-solid">{{ t('home.myTeams.cta') }}</RouterLink>
            </div>
          </v-card>

          <v-card border flat rounded="xl" class="pa-5 home-row-b-card" :style="rowBCardStyle">
            <h2 class="home-panel-title mb-3">{{ t('home.standings.title') }}</h2>

            <div class="home-activity-scroll">
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
                  :class="{
                    'home-standings-row--alt': index % 2 === 1,
                    'home-standings-row--unplayed': card.standing.played === 0,
                  }"
                >
                  <span class="home-standings-pos">{{ card.standing.position }}.º</span>
                  <span class="home-standings-team">{{ card.team.name }}</span>
                  <span class="text-right home-standings-muted">{{ card.standing.played }}</span>
                  <span class="text-right home-standings-points">{{ card.standing.points }}</span>
                </div>
              </template>
              <p v-else class="text-body-2 text-medium-emphasis">{{ t('home.standings.empty') }}</p>
            </div>
          </v-card>
        </div>
        </div>

        <aside class="home-side-col">
          <v-card border flat rounded="xl" class="pa-5 home-activity-panel" :style="activityPanelStyle">
            <h2 class="home-panel-title mb-5">{{ t('home.activity.title') }}</h2>

            <div class="home-activity-scroll">
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
            </div>
          </v-card>
        </aside>
      </div>
      </template>
    </v-container>
  </v-main>
</template>

<style scoped>
.home-outer-container {
  max-width: 1600px;
  margin-left: 0;
  margin-right: auto;
}

.home-layout {
  display: flex;
  align-items: stretch;
  gap: 24px;
}

.home-main-col {
  flex: 1 1 auto;
  max-width: 1200px;
  min-width: 0;
  /* Never let the sidebar's own (unbounded) content height stretch this
     column via home-layout's align-items - its height must stay purely a
     function of its own rows, since activityPanelStyle measures it and
     feeds that back into the sidebar card's explicit height. Without this,
     the two feed off each other and both grow without bound. */
  align-self: flex-start;
}

.home-side-col {
  flex: 0 0 340px;
  display: flex;
  align-self: flex-start;
}

.home-activity-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  padding-top: 10px !important;
  background: #f8fafc !important;
}

.home-kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.home-panel-title {
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  padding-bottom: 10px;
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

.home-hero-row {
  display: grid;
  grid-template-columns: 1.7fr 1fr;
  gap: 16px;
  align-items: stretch;
}

.home-row-b {
  display: grid;
  grid-template-columns: 1fr 1.7fr;
  gap: 16px;
  align-items: stretch;
}

.home-teams-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
  flex-shrink: 0;
}

.home-teams-header h2 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
}

.home-teams-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 22px;
  padding: 0 7px;
  border-radius: 999px;
  font-size: 11.5px;
  font-weight: 700;
}

.home-teams-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.home-teams-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 8px;
  border-radius: 12px;
  text-decoration: none;
  color: inherit;
}

.home-teams-row:hover {
  background: #f8fafc;
}

.home-teams-row-name {
  font-size: 13.5px;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.home-teams-row-tags {
  display: flex;
  align-items: center;
  gap: 5px;
  margin-top: 3px;
  overflow: hidden;
}

.home-teams-tag {
  flex-shrink: 0;
  padding: 2px 7px;
  border-radius: 999px;
  font-size: 9.5px;
  font-weight: 700;
  white-space: nowrap;
}

.home-teams-add {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  margin-top: 4px;
  padding: 8px;
  border-radius: 10px;
  border: 1px dashed #cbd5e1;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  text-decoration: none;
}

.home-teams-add:hover {
  border-color: #94a3b8;
  color: #475569;
}

.home-next-match-cta {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 32px;
  padding: 0 16px;
  border-radius: 10px;
  border: 1px dashed #16a34a;
  color: #16a34a;
  font-size: 12px;
  font-weight: 700;
  text-decoration: none;
}

.home-next-match-cta:hover {
  background: rgba(22, 163, 74, 0.08);
  border-color: #15803d;
  color: #15803d;
}

.home-next-match {
  display: flex;
  flex-direction: column;
  min-height: 96px;
}

.home-next-match-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
  flex-shrink: 0;
}

.home-next-match-header h2 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
}

.home-next-match-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 22px;
  padding: 0 7px;
  border-radius: 999px;
  font-size: 11.5px;
  font-weight: 700;
}

.home-next-match-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.home-next-match-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  border-radius: 12px;
  text-decoration: none;
  color: inherit;
}

.home-next-match-row:hover {
  background: #f8fafc;
}

.home-next-match-row--next {
  background: rgba(22, 163, 74, 0.08);
}

.home-next-match-row--next:hover {
  background: rgba(22, 163, 74, 0.1);
}

.home-next-match-date {
  width: 38px;
  height: 38px;
  flex-shrink: 0;
  border-radius: 10px;
  background: #f1f5f9;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.home-next-match-date--next {
  background: rgba(22, 163, 74, 0.14);
}

.home-next-match-month {
  font-size: 9.5px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: #64748b;
}

.home-next-match-date--next .home-next-match-month {
  color: rgb(var(--v-theme-primary));
}

.home-next-match-day {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 15px;
  font-weight: 700;
  line-height: 1;
  color: #64748b;
}

.home-next-match-date--next .home-next-match-day {
  color: rgb(var(--v-theme-primary));
}

.home-next-match-tag {
  font-size: 9.5px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: rgb(var(--v-theme-primary));
  margin-bottom: 1px;
}

.home-next-match-opponent {
  font-size: 13px;
  font-weight: 700;
  color: #0f172a;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.home-next-match-meta {
  font-size: 11px;
  color: #64748b;
  margin-top: 1px;
  display: flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
  overflow: hidden;
}

.home-next-match-venue {
  overflow: hidden;
  text-overflow: ellipsis;
}

.home-next-match-overflow {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  flex-shrink: 0;
  margin-top: 4px;
  padding: 7px;
  border-radius: 10px;
  border: 1px dashed #cbd5e1;
  color: #64748b;
  font-size: 11.5px;
  font-weight: 600;
  text-decoration: none;
}

.home-next-match-overflow:hover {
  border-color: #94a3b8;
  color: #475569;
}

.home-empty-block {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
}

.home-next-match .home-empty-block,
.home-teams-panel .home-empty-block {
  flex: 1 1 auto;
  align-items: center;
  justify-content: center;
  text-align: center;
  gap: 10px;
  padding-top: 8px;
}

.home-empty-icon {
  width: 42px;
  height: 42px;
  border-radius: 999px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
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

.home-standings-row--alt {
  background: #f5f9ff;
}

.home-standings-row--unplayed .home-standings-team {
  color: #64748b;
}

.home-standings-pos {
  font-weight: 700;
  color: #64748b;
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

.home-row-b-card {
  display: flex;
  flex-direction: column;
  min-height: 0;
  padding-top: 10px !important;
}

.home-activity-scroll {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 #f8fafc;
}

.home-activity-scroll::-webkit-scrollbar {
  width: 8px;
}

.home-activity-scroll::-webkit-scrollbar-track {
  background: #f8fafc;
}

.home-activity-scroll::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 999px;
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

@media (max-width: 1300px) {
  .home-layout {
    flex-direction: column;
  }

  .home-main-col {
    max-width: none;
  }

  .home-side-col {
    flex-basis: auto;
  }
}

@media (max-width: 960px) {
  .home-kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 899px) {
  .home-hero-row,
  .home-row-b {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 599px) {
  .home-kpi-grid {
    grid-template-columns: 1fr;
  }
}
</style>
