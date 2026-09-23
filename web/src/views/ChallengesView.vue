<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAlertOutline,
  mdiArrowDown,
  mdiArrowUp,
  mdiCalendarOutline,
  mdiDirections,
  mdiMapMarkerOutline,
  mdiSwordCross,
} from '@mdi/js'
import KitPreview from '@/components/KitPreview.vue'
import KitSwatch from '@/components/KitSwatch.vue'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { challengesApi, isActionablePending } from '@/lib/challenges'
import { DIVISION_COLOR } from '@/lib/division'
import { ApiError } from '@/lib/http'
import { relativeTime } from '@/lib/relativeTime'
import { tonalStyle } from '@/lib/tonalColor'
import { useAuthStore } from '@/stores/auth'
import { useChallengesStore } from '@/stores/challenges'
import { useUiStore } from '@/stores/ui'
import type { Challenge } from '@/types/challenge'
import type { TeamMemberRole } from '@/types/team'

const { t, locale } = useI18n()
const auth = useAuthStore()
const ui = useUiStore()
const challengesStore = useChallengesStore()
// xs (< 600px) matches the CSS's own @media (max-width: 599px) breakpoint,
// so the crest size switches in lockstep with the rest of the mobile layout.
const { xs } = useDisplay()

const loading = ref(true)
const challenges = ref<Challenge[]>([])
const roleByTeamId = ref<Record<string, TeamMemberRole>>({})
const actingId = ref<string | null>(null)

type Tab = 'all' | 'sent' | 'received'
const tab = ref<Tab>('all')
const selectedTeamId = ref<string | null>(null)
const selectedId = ref<string | null>(null)

const STATUS_COLOR: Record<Challenge['status'], string> = {
  Pending: '#D97706',
  Accepted: '#16a34a',
  Rejected: '#dc2626',
}

interface TeamRef {
  id: string
  name: string
  hasCrest: boolean
  category: Challenge['challengerTeamCategory']
  division: Challenge['challengerTeamDivision']
  classificationStyle: { category: string; division: string | null }
}

interface ChallengeItem {
  challenge: Challenge
  direction: 'sent' | 'received'
  myTeam: TeamRef
  otherTeamName: string
  homeTeam: TeamRef
  awayTeam: TeamRef
  statusStyle: string
  actionable: boolean
  matchDate: Date
  createdAt: Date
  respondedAt: Date | null
  routeUrl: string | null
}

function teamRef(
  id: string,
  name: string,
  hasCrest: boolean,
  category: Challenge['challengerTeamCategory'],
  division: Challenge['challengerTeamDivision'],
): TeamRef {
  return {
    id,
    name,
    hasCrest,
    category,
    division: division ?? null,
    classificationStyle: {
      category: tonalStyle(AGE_CATEGORY_COLOR[category]),
      division: division ? tonalStyle(DIVISION_COLOR[division]) : null,
    },
  }
}

/** The venue's own maps link when the team set one, else a Google Maps directions
 *  search built from its name/address - either way, a real route the browser can open. */
function buildRouteUrl(c: Challenge): string | null {
  if (c.venueMapsUrl) return c.venueMapsUrl
  if (!c.venueAddress) return null
  const destination = c.venueName ? `${c.venueName}, ${c.venueAddress}` : c.venueAddress
  return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}`
}

const items = computed<ChallengeItem[]>(() =>
  challenges.value.map((c) => {
    const iAmChallenger = c.challengerTeamId in roleByTeamId.value
    const direction: ChallengeItem['direction'] = iAmChallenger ? 'sent' : 'received'
    const challengerTeam = teamRef(c.challengerTeamId, c.challengerTeamName, c.challengerTeamHasCrest, c.challengerTeamCategory, c.challengerTeamDivision)
    const challengedTeam = teamRef(c.challengedTeamId, c.challengedTeamName, c.challengedTeamHasCrest, c.challengedTeamCategory, c.challengedTeamDivision)
    const homeTeam = c.homeTeamId === c.challengerTeamId ? challengerTeam : challengedTeam
    const awayTeam = homeTeam.id === c.challengerTeamId ? challengedTeam : challengerTeam

    return {
      challenge: c,
      direction,
      myTeam: iAmChallenger ? challengerTeam : challengedTeam,
      otherTeamName: iAmChallenger ? c.challengedTeamName : c.challengerTeamName,
      homeTeam,
      awayTeam,
      statusStyle: tonalStyle(STATUS_COLOR[c.status]),
      actionable: isActionablePending(c, roleByTeamId.value),
      matchDate: new Date(c.matchDate),
      createdAt: new Date(c.createdAt),
      respondedAt: c.respondedAt ? new Date(c.respondedAt) : null,
      routeUrl: buildRouteUrl(c),
    }
  }),
)

const pendingCount = computed(() => items.value.filter((i) => i.challenge.status === 'Pending').length)

// Keeps the nav badge in sync with whatever this view just loaded or changed,
// without a second fetch of its own - see stores/challenges.ts.
watch(
  () => items.value.filter((i) => i.actionable).length,
  (count) => challengesStore.setPendingCount(count),
  { immediate: true },
)

/** Only the user's own teams that actually have a challenge, so the filter never offers an
 *  empty result. */
const myTeamOptions = computed(() => {
  const byId = new Map<string, string>()
  for (const item of items.value) if (!byId.has(item.myTeam.id)) byId.set(item.myTeam.id, item.myTeam.name)
  return [...byId.entries()].map(([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name))
})

const filteredItems = computed(() =>
  items.value.filter(
    (i) => (tab.value === 'all' || i.direction === tab.value) && (!selectedTeamId.value || i.myTeam.id === selectedTeamId.value),
  ),
)
const selected = computed(
  () => filteredItems.value.find((i) => i.challenge.id === selectedId.value) ?? filteredItems.value[0] ?? null,
)

const tabs = computed(() => [
  { key: 'all' as const, label: t('challenges.list.tabAll') },
  { key: 'sent' as const, label: t('challenges.list.tabSent') },
  { key: 'received' as const, label: t('challenges.list.tabReceived') },
])

function selectTab(key: Tab): void {
  tab.value = key
}

async function respond(item: ChallengeItem, action: 'accept' | 'reject'): Promise<void> {
  actingId.value = item.challenge.id
  try {
    const updated =
      action === 'accept'
        ? await challengesApi.accept(item.challenge.id, auth.accessToken)
        : await challengesApi.reject(item.challenge.id, auth.accessToken)
    challenges.value = challenges.value.map((c) => (c.id === updated.id ? updated : c))
    ui.notify(t(`challenges.list.${action === 'accept' ? 'acceptSuccess' : 'rejectSuccess'}`, { team: item.otherTeamName }))
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('challenges.list.actionFailed'), 'error')
  } finally {
    actingId.value = null
  }
}

onMounted(async () => {
  try {
    await auth.loadMyTeams()
    const memberships = auth.myTeams
    roleByTeamId.value = Object.fromEntries(memberships.map((m) => [m.teamId, m.role]))

    const lists = await Promise.all(memberships.map((m) => challengesApi.forTeam(m.teamId, auth.accessToken)))
    const byId = new Map<string, Challenge>()
    for (const list of lists) for (const c of list) byId.set(c.id, c)
    challenges.value = [...byId.values()].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('challenges.list.loadFailed'), 'error')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <v-main>
    <v-container fluid class="py-6 py-md-8 challenges-outer-container">
      <v-progress-circular v-if="loading" indeterminate color="primary" class="d-block mx-auto my-16" />

      <template v-else-if="items.length === 0">
        <div class="challenges-empty">
          <div class="challenges-empty-icon">
            <v-icon :icon="mdiSwordCross" size="26" color="#94a3b8" />
          </div>
          <p class="text-body-1 text-medium-emphasis">{{ t('challenges.list.empty') }}</p>
        </div>
      </template>

      <template v-else>
        <div class="challenges-layout">
          <!-- LEFT: list -->
          <div class="challenges-list-col">
            <div class="challenges-list-header">
              <h1 class="challenges-title">{{ t('challenges.list.title') }}</h1>
              <p class="challenges-count">{{ t('challenges.list.countSummary', { count: filteredItems.length, pending: pendingCount }) }}</p>
              <div class="challenges-tabs">
                <button
                  v-for="tabDef in tabs"
                  :key="tabDef.key"
                  type="button"
                  class="challenges-tab"
                  :class="{ 'challenges-tab--active': tab === tabDef.key }"
                  @click="selectTab(tabDef.key)"
                >
                  {{ tabDef.label }}
                </button>
              </div>
              <v-select
                v-model="selectedTeamId"
                :items="myTeamOptions"
                item-title="name"
                item-value="id"
                :label="t('challenges.list.filterTeam')"
                variant="outlined"
                density="compact"
                clearable
                hide-details
                class="challenges-team-filter"
              />
            </div>

            <div class="challenges-list-scroll">
              <p v-if="filteredItems.length === 0" class="text-body-2 text-medium-emphasis challenges-list-empty">
                {{ t('challenges.list.emptyFiltered') }}
              </p>
              <button
                v-for="item in filteredItems"
                :key="item.challenge.id"
                type="button"
                class="challenges-row"
                :class="{ 'challenges-row--selected': selected?.challenge.id === item.challenge.id }"
                @click="selectedId = item.challenge.id"
              >
                <div class="challenges-row-top">
                  <div class="challenges-row-teams">
                    <TeamCrest :team="item.homeTeam" :size="20" />
                    <span class="challenges-row-vs">vs</span>
                    <TeamCrest :team="item.awayTeam" :size="20" />
                    <span class="challenges-row-name">{{ item.otherTeamName }}</span>
                  </div>
                  <span class="challenges-status-pill" :style="item.statusStyle">{{ t(`challenges.status.${item.challenge.status}`) }}</span>
                </div>
                <div class="challenges-row-tags">
                  <span class="challenges-row-my-team">{{ item.myTeam.name }}</span>
                  <span class="challenges-tag" :style="item.myTeam.classificationStyle.category">{{ t(`profile.team.enums.${item.myTeam.category}`) }}</span>
                  <span v-if="item.myTeam.division" class="challenges-tag" :style="item.myTeam.classificationStyle.division!">
                    {{ t(`profile.team.enums.${item.myTeam.division}`) }}
                  </span>
                </div>
                <div class="challenges-row-meta">
                  <v-icon :icon="mdiCalendarOutline" size="12" />
                  <span>{{ item.matchDate.toLocaleDateString(locale, { day: 'numeric', month: 'short' }) }}</span>
                  <span>·</span>
                  <span class="challenges-row-venue">{{ item.challenge.venueName ?? '—' }}</span>
                  <span class="challenges-row-direction">
                    <v-icon :icon="item.direction === 'sent' ? mdiArrowUp : mdiArrowDown" size="11" />
                    {{ t(item.direction === 'sent' ? 'challenges.list.directionSent' : 'challenges.list.directionReceived') }}
                  </span>
                </div>
              </button>
            </div>
          </div>

          <!-- RIGHT: detail -->
          <div v-if="selected" class="challenges-detail-col">
            <div class="challenges-detail-scroll" :class="{ 'challenges-detail-scroll--actionable': selected.actionable }">
              <div class="challenges-detail-header">
                <span class="challenges-status-pill challenges-status-pill--lg" :style="selected.statusStyle">
                  {{ t(`challenges.status.${selected.challenge.status}`) }}
                </span>
                <span class="challenges-detail-direction">
                  <v-icon :icon="selected.direction === 'sent' ? mdiArrowUp : mdiArrowDown" size="13" />
                  {{ t(selected.direction === 'sent' ? 'challenges.list.directionSent' : 'challenges.list.directionReceived') }}
                </span>
              </div>

              <div class="challenges-teams-row">
                <div class="challenges-team-col">
                  <TeamCrest :team="selected.homeTeam" :size="xs ? 52 : 64" />
                  <div class="challenges-team-name">{{ selected.homeTeam.name }}</div>
                  <div class="challenges-team-tag">{{ t('challenges.list.home') }}</div>
                  <div class="challenges-team-classification">
                    <span class="challenges-tag" :style="selected.homeTeam.classificationStyle.category">{{ t(`profile.team.enums.${selected.homeTeam.category}`) }}</span>
                    <span v-if="selected.homeTeam.division" class="challenges-tag" :style="selected.homeTeam.classificationStyle.division!">
                      {{ t(`profile.team.enums.${selected.homeTeam.division}`) }}
                    </span>
                  </div>
                </div>
                <div class="challenges-vs">VS</div>
                <div class="challenges-team-col">
                  <TeamCrest :team="selected.awayTeam" :size="xs ? 52 : 64" />
                  <div class="challenges-team-name">{{ selected.awayTeam.name }}</div>
                  <div class="challenges-team-tag">{{ t('challenges.list.away') }}</div>
                  <div class="challenges-team-classification">
                    <span class="challenges-tag" :style="selected.awayTeam.classificationStyle.category">{{ t(`profile.team.enums.${selected.awayTeam.category}`) }}</span>
                    <span v-if="selected.awayTeam.division" class="challenges-tag" :style="selected.awayTeam.classificationStyle.division!">
                      {{ t(`profile.team.enums.${selected.awayTeam.division}`) }}
                    </span>
                  </div>
                </div>
              </div>

              <div class="challenges-info-grid">
                <div class="challenges-info-card">
                  <div class="challenges-info-icon">
                    <v-icon :icon="mdiCalendarOutline" size="17" color="#4F46E5" />
                  </div>
                  <div>
                    <div class="challenges-info-label">{{ t('challenges.list.dateTime') }}</div>
                    <div class="challenges-info-value">
                      {{ selected.matchDate.toLocaleDateString(locale, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }) }}
                      ·
                      {{ selected.matchDate.toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit' }) }}
                    </div>
                  </div>
                </div>
                <div class="challenges-info-card">
                  <div class="challenges-info-icon">
                    <v-icon :icon="mdiMapMarkerOutline" size="17" color="#4F46E5" />
                  </div>
                  <div class="min-width-0">
                    <div class="challenges-info-label">{{ t('challenges.list.venue') }}</div>
                    <template v-if="selected.challenge.venueName">
                      <div class="challenges-info-value">{{ selected.challenge.venueName }}</div>
                      <div class="challenges-info-sub">
                        <template v-if="selected.challenge.venueAddress">{{ selected.challenge.venueAddress }} · </template>
                        <template v-if="selected.challenge.venueSurface">{{ t(`profile.team.surfaces.${selected.challenge.venueSurface}`) }}</template>
                      </div>
                      <a v-if="selected.routeUrl" :href="selected.routeUrl" target="_blank" rel="noopener" class="challenges-route-btn">
                        <v-icon :icon="mdiDirections" size="15" />
                        {{ t('challenges.list.viewRoute') }}
                      </a>
                    </template>
                    <div v-else class="challenges-info-value challenges-info-value--muted">{{ t('challenges.wizard.venueUndefined') }}</div>
                  </div>
                </div>
              </div>

              <div class="challenges-section-title">{{ t('challenges.list.kits') }}</div>
              <div class="challenges-kits-grid">
                <div class="challenges-kit-card">
                  <template v-if="selected.challenge.homeKit">
                    <KitPreview
                      :pattern="selected.challenge.homeKit.kitPattern"
                      :primary="selected.challenge.homeKit.colorPrimary"
                      :secondary="selected.challenge.homeKit.colorSecondary"
                      :shorts="selected.challenge.homeKit.shortsColor"
                      :size="52"
                    />
                  </template>
                  <div v-else class="challenges-kit-empty">
                    <KitSwatch pattern="Plain" primary="#f1f5f9" secondary="#f1f5f9" :size="40" />
                  </div>
                  <div>
                    <div class="challenges-kit-label">{{ t('challenges.list.home') }} · {{ selected.homeTeam.name }}</div>
                    <div class="challenges-kit-pattern">
                      {{ selected.challenge.homeKit ? t(`profile.team.kitPatterns.${selected.challenge.homeKit.kitPattern}`) : t('challenges.wizard.kitNotSet') }}
                    </div>
                  </div>
                </div>
                <div class="challenges-kit-card">
                  <template v-if="selected.challenge.awayKit">
                    <KitPreview
                      :pattern="selected.challenge.awayKit.kitPattern"
                      :primary="selected.challenge.awayKit.colorPrimary"
                      :secondary="selected.challenge.awayKit.colorSecondary"
                      :shorts="selected.challenge.awayKit.shortsColor"
                      :size="52"
                    />
                  </template>
                  <div v-else class="challenges-kit-empty">
                    <KitSwatch pattern="Plain" primary="#f1f5f9" secondary="#f1f5f9" :size="40" />
                  </div>
                  <div>
                    <div class="challenges-kit-label">{{ t('challenges.list.away') }} · {{ selected.awayTeam.name }}</div>
                    <div class="challenges-kit-pattern">
                      {{ selected.challenge.awayKit ? t(`profile.team.kitPatterns.${selected.challenge.awayKit.kitPattern}`) : t('challenges.wizard.kitNotSet') }}
                    </div>
                  </div>
                </div>
              </div>

              <div v-if="selected.challenge.kitsClash" class="challenges-clash-banner">
                <v-icon :icon="mdiAlertOutline" size="16" color="#B45309" />
                <span>{{ t('challenges.wizard.kitInfoClash') }}</span>
              </div>

              <template v-if="selected.challenge.message">
                <div class="challenges-section-title">{{ t('challenges.list.message') }}</div>
                <div class="challenges-message">{{ selected.challenge.message }}</div>
              </template>

              <div class="challenges-timestamps">
                <span>{{ t('challenges.list.sentRelative', { time: relativeTime(selected.createdAt, locale) }) }}</span>
                <span v-if="selected.respondedAt">{{ t('challenges.list.respondedRelative', { time: relativeTime(selected.respondedAt, locale) }) }}</span>
              </div>
            </div>

            <div v-if="selected.actionable" class="challenges-actions-bar">
              <button
                type="button"
                class="fp-btn challenges-btn-reject"
                :disabled="actingId === selected.challenge.id"
                @click="respond(selected, 'reject')"
              >
                {{ t('challenges.list.reject') }}
              </button>
              <button
                type="button"
                class="fp-btn fp-btn-solid"
                :disabled="actingId === selected.challenge.id"
                @click="respond(selected, 'accept')"
              >
                {{ t('challenges.list.accept') }}
              </button>
            </div>
          </div>
        </div>
      </template>
    </v-container>
  </v-main>
</template>

<style scoped>
.challenges-outer-container {
  max-width: 1600px;
  margin-inline: auto;
}

.challenges-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  padding: 80px 20px;
}

.challenges-empty-icon {
  width: 56px;
  height: 56px;
  border-radius: 999px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
}

.challenges-layout {
  display: flex;
  gap: 20px;
  height: calc(100vh - 180px);
  min-height: 560px;
}

.challenges-list-col {
  width: 400px;
  flex-shrink: 0;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
}

.challenges-list-header {
  flex-shrink: 0;
  padding: 22px 18px 14px;
  border-bottom: 1px solid #f1f5f9;
}

.challenges-title {
  margin: 0;
  font-size: 19px;
  font-weight: 700;
  color: #0f172a;
  font-family: 'Space Grotesk', sans-serif;
}

.challenges-count {
  margin: 4px 0 14px;
  font-size: 12.5px;
  color: #64748b;
}

.challenges-tabs {
  display: flex;
  gap: 6px;
}

.challenges-tab {
  padding: 7px 13px;
  border-radius: 999px;
  border: none;
  background: transparent;
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
}

.challenges-tab:hover {
  background: #f8fafc;
}

.challenges-tab--active,
.challenges-tab--active:hover {
  background: rgba(79, 70, 229, 0.14);
  color: #4F46E5;
}

.challenges-team-filter {
  margin-top: 12px;
}

.challenges-tag {
  flex-shrink: 0;
  padding: 2px 7px;
  border-radius: 999px;
  font-size: 9px;
  font-weight: 700;
  white-space: nowrap;
}

.challenges-list-scroll {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding: 6px 10px 14px;
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 transparent;
}

.challenges-list-scroll::-webkit-scrollbar {
  width: 8px;
}

.challenges-list-scroll::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 999px;
}

.challenges-list-empty {
  padding: 16px 8px;
}

.challenges-row {
  display: block;
  width: 100%;
  text-align: left;
  cursor: pointer;
  padding: 11px 10px;
  border-radius: 14px;
  margin-bottom: 2px;
  border: none;
  background: transparent;
  font: inherit;
  color: inherit;
}

.challenges-row:hover {
  background: #f8fafc;
}

.challenges-row--selected,
.challenges-row--selected:hover {
  background: rgba(79, 70, 229, 0.08);
}

.challenges-row-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.challenges-row-teams {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.challenges-row-vs {
  font-size: 10px;
  color: #94a3b8;
  flex-shrink: 0;
}

.challenges-row-name {
  font-size: 13px;
  font-weight: 700;
  color: #0f172a;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.challenges-status-pill {
  flex-shrink: 0;
  padding: 2px 9px;
  border-radius: 999px;
  font-size: 9.5px;
  font-weight: 700;
  white-space: nowrap;
}

.challenges-status-pill--lg {
  padding: 4px 12px;
  font-size: 11.5px;
}

.challenges-row-tags {
  display: flex;
  align-items: center;
  gap: 5px;
  margin-top: 5px;
}

.challenges-row-my-team {
  font-size: 10.5px;
  color: #94a3b8;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 110px;
}

.challenges-row-meta {
  display: flex;
  align-items: center;
  gap: 5px;
  margin-top: 6px;
  font-size: 11px;
  color: #64748b;
}

.challenges-row-venue {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.challenges-row-direction {
  margin-left: auto;
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  gap: 3px;
  color: #94a3b8;
}

.challenges-detail-col {
  flex: 1 1 auto;
  min-width: 0;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
  position: relative;
}

.challenges-detail-scroll {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding: 32px 36px 32px;
}

.challenges-detail-scroll--actionable {
  padding-bottom: 96px;
}

.challenges-detail-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 24px;
}

.challenges-detail-direction {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #64748b;
}

.challenges-teams-row {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 28px;
  margin-bottom: 28px;
}

.challenges-team-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  width: 220px;
}

.challenges-team-name {
  font-size: 14.5px;
  font-weight: 700;
  color: #0f172a;
  text-align: center;
}

.challenges-team-tag {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: #64748b;
}

.challenges-team-classification {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-wrap: nowrap;
  gap: 5px;
  margin-top: 2px;
}

.challenges-vs {
  font-family: 'Space Grotesk', sans-serif;
  font-size: 18px;
  font-weight: 700;
  color: #cbd5e1;
}

.challenges-info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.challenges-info-card {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 14px 16px;
  background: #f8fafc;
  border-radius: 14px;
}

.challenges-info-icon {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  background: rgba(79, 70, 229, 0.14);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.challenges-info-label {
  font-size: 10.5px;
  font-weight: 700;
  letter-spacing: 0.03em;
  text-transform: uppercase;
  color: #94a3b8;
}

.challenges-info-value {
  font-size: 13.5px;
  font-weight: 700;
  color: #0f172a;
  margin-top: 2px;
}

.challenges-info-value--muted {
  color: #64748b;
  font-weight: 600;
}

.challenges-info-sub {
  font-size: 11.5px;
  color: #64748b;
  margin-top: 1px;
}

.challenges-route-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: 8px;
  padding: 7px 14px;
  border-radius: 9px;
  background: rgba(79, 70, 229, 0.14);
  color: #4F46E5;
  font-size: 12px;
  font-weight: 700;
  text-decoration: none;
}

.challenges-route-btn:hover {
  background: rgba(79, 70, 229, 0.22);
}

.challenges-section-title {
  margin-bottom: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #0f172a;
}

.challenges-kits-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 12px;
}

.challenges-kit-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 16px;
  background: #f8fafc;
  border-radius: 14px;
}

.challenges-kit-empty {
  flex-shrink: 0;
}

.challenges-kit-label {
  font-size: 12.5px;
  font-weight: 700;
  color: #0f172a;
}

.challenges-kit-pattern {
  font-size: 11.5px;
  color: #64748b;
  margin-top: 2px;
}

.challenges-clash-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 11px 16px;
  background: rgba(217, 119, 6, 0.1);
  border: 1px solid rgba(217, 119, 6, 0.3);
  border-radius: 12px;
  margin-bottom: 20px;
  font-size: 12.5px;
  color: #92400e;
}

.challenges-message {
  padding: 16px;
  background: #f8fafc;
  border-radius: 14px;
  font-size: 13.5px;
  color: #334155;
  line-height: 1.5;
  margin-bottom: 20px;
}

.challenges-timestamps {
  display: flex;
  gap: 20px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
  font-size: 11.5px;
  color: #94a3b8;
}

.challenges-btn-reject {
  background: #ffffff;
  border: 1.5px solid #fecaca;
  color: #dc2626;
}

.challenges-btn-reject:hover:not(:disabled) {
  background: #fef2f2;
  border-color: #fca5a5;
}

.challenges-actions-bar {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  padding: 16px 36px;
  background: #ffffff;
  border-top: 1px solid #e2e8f0;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

@media (max-width: 1100px) {
  .challenges-layout {
    flex-direction: column;
    height: auto;
  }

  .challenges-list-col {
    width: 100%;
    max-height: 420px;
  }

  .challenges-detail-col {
    min-height: 480px;
  }
}

/* Real phone widths: the 1100px breakpoint above only stacks list and detail,
   it doesn't touch any internal sizing, so the team-vs-team row (two fixed
   220px columns + 28px gap), the 2-column info/kits grids and the detail
   panel's desktop padding all stay too wide for a ~390px screen. */
@media (max-width: 599px) {
  .challenges-list-col {
    max-height: 320px;
  }

  .challenges-list-header {
    padding: 16px 14px 12px;
  }

  .challenges-detail-scroll {
    padding: 20px 16px;
  }

  .challenges-detail-scroll--actionable {
    padding-bottom: 88px;
  }

  .challenges-teams-row {
    gap: 14px;
    margin-bottom: 24px;
  }

  .challenges-team-col {
    width: auto;
    flex: 1 1 0;
    min-width: 0;
    max-width: 150px;
    gap: 6px;
  }

  .challenges-team-classification {
    flex-wrap: wrap;
  }

  .challenges-info-grid,
  .challenges-kits-grid {
    grid-template-columns: 1fr;
  }

  .challenges-actions-bar {
    padding: 14px 16px;
    justify-content: stretch;
  }

  .challenges-actions-bar .fp-btn {
    flex: 1;
  }
}
</style>
