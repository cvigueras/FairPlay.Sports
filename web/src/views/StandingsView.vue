<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import { mdiArrowDown, mdiArrowUp, mdiTrophyOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { standingsApi } from '@/lib/standings'
import { useAuthStore } from '@/stores/auth'
import TeamCrest from '@/components/TeamCrest.vue'
import type { Standing } from '@/types/standing'
import type { PagedResult } from '@/types/pagination'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  type AgeCategory,
  type Division,
  type FootballType,
} from '@/types/team'

const { t } = useI18n()
const auth = useAuthStore()
const { smAndDown } = useDisplay()

const PAGE_SIZE = 20

const page = ref(1)
const result = ref<PagedResult<Standing> | null>(null)
const loading = ref(false)
const error = ref('')

// Empty (null) means "no filter"; changing one re-queries from page 1.
const type = ref<FootballType | null>(null)
const division = ref<Division | null>(null)
const category = ref<AgeCategory | null>(null)

const enumItems = <T extends string>(values: readonly T[]) =>
  values.map((value) => ({ value, title: t(`profile.team.enums.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES))
const divisionItems = computed(() => enumItems(DIVISIONS))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES))

/** Sortable columns, in display order. Points defaults to best-first, like any league table. */
const COLUMNS = [
  { field: 'points', labelKey: 'standings.fields.points' },
  { field: 'played', labelKey: 'standings.fields.played' },
  { field: 'won', labelKey: 'standings.fields.won' },
  { field: 'drawn', labelKey: 'standings.fields.drawn' },
  { field: 'lost', labelKey: 'standings.fields.lost' },
  { field: 'goalsfor', labelKey: 'standings.fields.goalsFor' },
  { field: 'goalsagainst', labelKey: 'standings.fields.goalsAgainst' },
  { field: 'goaldifference', labelKey: 'standings.fields.goalDifference' },
] as const

const sortField = ref<string>('points')
const sortDescending = ref(true)
const sort = computed(() => `${sortDescending.value ? '-' : ''}${sortField.value}`)

function toggleSort(field: string) {
  if (sortField.value === field) {
    sortDescending.value = !sortDescending.value
  } else {
    sortField.value = field
    sortDescending.value = true
  }
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    result.value = await standingsApi.page(
      {
        page: page.value,
        pageSize: PAGE_SIZE,
        sort: sort.value,
        type: type.value ?? undefined,
        division: division.value ?? undefined,
        category: category.value ?? undefined,
      },
      auth.accessToken,
    )
  } catch (err) {
    error.value = err instanceof ApiError ? err.message : t('standings.loadFailed')
  } finally {
    loading.value = false
  }
}

// A filter or sort change goes back to the first page; reload directly if already there.
function reload() {
  if (page.value === 1) load()
  else page.value = 1
}

watch(page, load, { immediate: true })
watch([sort, type, division, category], reload)

/** The table's running row number, independent of the current page. */
function positionOf(index: number): number {
  return (page.value - 1) * PAGE_SIZE + index + 1
}

/** A podium medal for the top 3 overall positions; plain otherwise. */
function medalClass(position: number): string {
  if (position === 1) return 'standings-medal--gold'
  if (position === 2) return 'standings-medal--silver'
  if (position === 3) return 'standings-medal--bronze'
  return ''
}
</script>

<template>
  <v-main>
    <div class="standings-page">
      <div class="standings-header">
        <h1 class="text-h5 font-weight-bold d-flex align-center ga-2">
          <v-icon :icon="mdiTrophyOutline" color="#C9A227" />
          {{ t('standings.title') }}
        </h1>

        <div class="standings-filterbar">
          <v-select
            v-model="type"
            :items="typeItems"
            :label="t('teams.fields.type')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
            class="standings-filter-select"
          />
          <v-select
            v-model="division"
            :items="divisionItems"
            :label="t('teams.fields.division')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
            class="standings-filter-select"
          />
          <v-select
            v-model="category"
            :items="categoryItems"
            :label="t('teams.fields.category')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
            class="standings-filter-select"
          />
        </div>
      </div>

      <div class="standings-list">
        <v-progress-circular
          v-if="loading"
          indeterminate
          color="primary"
          class="d-block mx-auto my-16"
        />
        <v-alert v-else-if="error" type="error" variant="tonal">{{ error }}</v-alert>

        <template v-else-if="result">
          <div v-if="result.items.length === 0" class="standings-empty text-medium-emphasis">
            <v-icon :icon="mdiTrophyOutline" size="48" />
            <p class="text-body-1 mt-3">{{ t('standings.empty') }}</p>
          </div>

          <div v-else class="standings-table-wrap">
            <div class="standings-card">
              <table class="standings-table">
                <thead>
                  <tr>
                    <th class="standings-col-position">{{ t('standings.fields.position') }}</th>
                    <th class="standings-col-club">{{ t('standings.fields.club') }}</th>
                    <th
                      v-for="column in COLUMNS"
                      :key="column.field"
                      class="standings-col-stat"
                      :class="{ 'standings-col-stat--active': sortField === column.field }"
                      @click="toggleSort(column.field)"
                    >
                      <span class="standings-th-inner">
                        {{ t(column.labelKey) }}
                        <v-icon
                          v-if="sortField === column.field"
                          :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                          size="14"
                        />
                      </span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(standing, index) in result.items" :key="standing.id">
                    <td class="standings-col-position">
                      <span class="standings-medal" :class="medalClass(positionOf(index))">
                        {{ positionOf(index) }}
                      </span>
                    </td>
                    <td class="standings-col-club">
                      <RouterLink
                        :to="{ name: 'team-detail', params: { id: standing.teamId } }"
                        class="standings-club-link"
                      >
                        <TeamCrest
                          :team="{ id: standing.teamId, name: standing.teamName, hasCrest: standing.teamHasCrest }"
                          :size="30"
                        />
                        <span class="text-truncate">{{ standing.teamName }}</span>
                      </RouterLink>
                    </td>
                    <td class="standings-col-stat">
                      <span class="standings-points">{{ standing.points }}</span>
                    </td>
                    <td class="standings-col-stat">{{ standing.played }}</td>
                    <td class="standings-col-stat">{{ standing.won }}</td>
                    <td class="standings-col-stat">{{ standing.drawn }}</td>
                    <td class="standings-col-stat">{{ standing.lost }}</td>
                    <td class="standings-col-stat">{{ standing.goalsFor }}</td>
                    <td class="standings-col-stat">{{ standing.goalsAgainst }}</td>
                    <td
                      class="standings-col-stat standings-col-stat--goaldiff"
                      :class="{
                        'text-success': standing.goalDifference > 0,
                        'text-error': standing.goalDifference < 0,
                      }"
                    >
                      {{ standing.goalDifference > 0 ? '+' : '' }}{{ standing.goalDifference }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </template>
      </div>

      <footer v-if="result" class="standings-footer">
        <div class="standings-footer-inner">
          <v-pagination
            v-if="result.totalPages > 1"
            v-model="page"
            :length="result.totalPages"
            :total-visible="smAndDown ? 3 : 7"
            show-first-last-page
            density="comfortable"
            variant="text"
            active-color="primary"
            class="app-pagination"
          />
          <p class="standings-count font-weight-bold">
            {{ t('standings.count', { n: result.totalCount }) }}
          </p>
        </div>
      </footer>
    </div>
  </v-main>
</template>

<style scoped>
/* Fill the space under the app bar; only the list scrolls, header and pager stay put. */
.standings-page {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: calc(100dvh - var(--v-layout-top, 64px));
  overflow: hidden;
  padding-top: 1.5rem;
}

/* Matches the max-width the other list pages (Teams) use, so navigating
   between them doesn't shift the content edges. The table itself stays
   capped narrower below - it has nothing to gain from the extra room. */
.standings-header,
.standings-list,
.standings-footer-inner {
  width: 100%;
  max-width: 1600px;
  margin-inline: auto;
  padding-inline: 1.5rem;
}

.standings-header {
  flex: 0 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-bottom: 1rem;
}

.standings-filterbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.standings-filter-select {
  flex: 1 1 160px;
  max-width: 220px;
}

.standings-list {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding-bottom: 1rem;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.standings-list::-webkit-scrollbar {
  width: 0;
  height: 0;
}

.standings-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 4rem 1rem;
}

/* Narrow screens can't fit ten columns - let the table itself scroll sideways
   rather than squeezing every column unreadably thin. */
.standings-table-wrap {
  overflow-x: auto;
}

/* One elevated card wrapping the table, instead of a bare table sitting
   directly on the page background. Slate palette to match the rest of the
   app's redesigned screens (login, shell, teams) rather than Vuetify's
   generic (black-based) theme tokens. The card spans the full width of the
   page (same as the header/footer above and Teams' own content), while the
   table itself stays capped and centred inside it - stretching every stat
   column to fill 1600px would leave them swimming in dead space. */
.standings-card {
  width: 100%;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  overflow: hidden;
}

.standings-table {
  width: 100%;
  max-width: 1040px;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.standings-table thead th {
  position: sticky;
  top: 0;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  padding: 0.75rem 0.85rem;
  text-align: center;
  font-size: 0.6875rem;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  white-space: nowrap;
  user-select: none;
}

.standings-table thead th.standings-col-club {
  text-align: left;
  padding-inline: 0.5rem;
}

.standings-table thead th.standings-col-stat {
  cursor: pointer;
}

.standings-th-inner {
  display: inline-flex;
  align-items: center;
  gap: 0.15rem;
}

.standings-col-stat--active {
  color: #15803d;
}

.standings-table tbody tr:not(:last-child) td {
  border-bottom: 1px solid #f1f5f9;
}

.standings-table td {
  padding: 0.7rem 0.85rem;
}

.standings-table td.standings-col-club {
  padding-inline: 0.5rem;
}

.standings-col-position {
  width: 3.5rem;
  text-align: center;
}

.standings-medal {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 50%;
  background: #f1f5f9;
  color: #64748b;
  font-weight: 700;
  font-size: 0.8125rem;
}

.standings-medal--gold {
  background: rgba(234, 179, 8, 0.16);
  color: #a16207;
}

.standings-medal--silver {
  background: rgba(148, 163, 184, 0.22);
  color: #475569;
}

.standings-medal--bronze {
  background: rgba(180, 83, 9, 0.14);
  color: #9a3412;
}

.standings-col-club {
  min-width: 220px;
}

.standings-col-stat {
  width: 4rem;
  text-align: center;
  color: #475569;
}

.standings-col-stat--goaldiff {
  font-weight: 700;
}

.standings-points {
  display: inline-flex;
  padding: 0.15rem 0.7rem;
  border-radius: 999px;
  background: #dcfce7;
  color: #15803d;
  font-weight: 700;
}

.standings-club-link {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #0f172a;
  text-decoration: none;
  min-width: 0;
  font-weight: 600;
}

.standings-club-link:hover {
  color: #16a34a;
  text-decoration: underline;
}

.standings-footer {
  flex: 0 0 auto;
  border-top: 1px solid #e2e8f0;
  background: #ffffff;
}

.standings-footer-inner {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 3rem;
  padding-block: 0.75rem 1rem;
}

.standings-count {
  position: absolute;
  right: 1.5rem;
  margin: 0;
}

/* Pill-style pager matching the app's buttons: a filled green circle for the
   current page, bordered icon buttons for first/prev/next/last. */
.app-pagination :deep(.v-pagination__item .v-btn),
.app-pagination :deep(.v-pagination__first .v-btn),
.app-pagination :deep(.v-pagination__prev .v-btn),
.app-pagination :deep(.v-pagination__next .v-btn),
.app-pagination :deep(.v-pagination__last .v-btn) {
  border-radius: 10px !important;
}

.app-pagination :deep(.v-pagination__item .v-btn) {
  color: #475569;
  font-weight: 600;
}

.app-pagination :deep(.v-pagination__item--is-active .v-btn) {
  background: #16a34a !important;
  color: #ffffff !important;
}

.app-pagination :deep(.v-pagination__first .v-btn),
.app-pagination :deep(.v-pagination__prev .v-btn),
.app-pagination :deep(.v-pagination__next .v-btn),
.app-pagination :deep(.v-pagination__last .v-btn) {
  border: 1.5px solid #e2e8f0;
  color: #334155;
}

@media (max-width: 599px) {
  .standings-footer-inner {
    flex-direction: column;
    gap: 0.35rem;
  }

  .standings-count {
    position: static;
  }

  .standings-footer-inner :deep(.v-pagination) {
    margin-inline: 1.5rem;
  }
}
</style>
