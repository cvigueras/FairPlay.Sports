<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import { mdiArrowDown, mdiArrowUp, mdiShieldOutline, mdiTrophyOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { standingsApi } from '@/lib/standings'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import type { Standing } from '@/types/standing'
import type { PagedResult } from '@/types/pagination'

const { t } = useI18n()
const auth = useAuthStore()
const { smAndDown } = useDisplay()

const PAGE_SIZE = 20

const page = ref(1)
const result = ref<PagedResult<Standing> | null>(null)
const loading = ref(false)
const error = ref('')

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
      { page: page.value, pageSize: PAGE_SIZE, sort: sort.value },
      auth.accessToken,
    )
  } catch (err) {
    error.value = err instanceof ApiError ? err.message : t('standings.loadFailed')
  } finally {
    loading.value = false
  }
}

function reload() {
  if (page.value === 1) load()
  else page.value = 1
}

watch(page, load, { immediate: true })
watch(sort, reload)

/** The table's running row number, independent of the current page. */
function positionOf(index: number): number {
  return (page.value - 1) * PAGE_SIZE + index + 1
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
                  <td class="standings-col-position text-medium-emphasis">{{ positionOf(index) }}</td>
                  <td class="standings-col-club">
                    <RouterLink
                      :to="{ name: 'team-detail', params: { id: standing.teamId } }"
                      class="standings-club-link"
                    >
                      <v-avatar size="28" rounded="0" color="transparent">
                        <v-img v-if="standing.teamHasCrest" :src="teamsApi.crestUrl(standing.teamId)" :alt="standing.teamName" />
                        <v-icon v-else :icon="mdiShieldOutline" size="22" class="text-medium-emphasis" />
                      </v-avatar>
                      <span class="text-truncate">{{ standing.teamName }}</span>
                    </RouterLink>
                  </td>
                  <td class="standings-col-stat font-weight-bold">{{ standing.points }}</td>
                  <td class="standings-col-stat">{{ standing.played }}</td>
                  <td class="standings-col-stat">{{ standing.won }}</td>
                  <td class="standings-col-stat">{{ standing.drawn }}</td>
                  <td class="standings-col-stat">{{ standing.lost }}</td>
                  <td class="standings-col-stat">{{ standing.goalsFor }}</td>
                  <td class="standings-col-stat">{{ standing.goalsAgainst }}</td>
                  <td
                    class="standings-col-stat"
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
        </template>
      </div>

      <footer v-if="result" class="standings-footer">
        <div class="standings-footer-inner">
          <v-pagination
            v-if="result.totalPages > 1"
            v-model="page"
            :length="result.totalPages"
            :total-visible="smAndDown ? 3 : 7"
            rounded="circle"
            density="comfortable"
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

.standings-header,
.standings-list,
.standings-footer-inner {
  width: 100%;
  max-width: 1100px;
  margin-inline: auto;
  padding-inline: 1.5rem;
}

.standings-header {
  flex: 0 0 auto;
  padding-bottom: 1rem;
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

.standings-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.standings-table thead th {
  position: sticky;
  top: 0;
  background: rgb(var(--v-theme-surface));
  border-bottom: 2px solid rgb(var(--v-theme-primary));
  padding: 0.6rem 0.5rem;
  text-align: center;
  font-weight: 700;
  white-space: nowrap;
  user-select: none;
}

.standings-table thead th.standings-col-club {
  text-align: left;
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
  color: rgb(var(--v-theme-primary));
}

.standings-table tbody tr:nth-child(even) {
  background: rgba(var(--v-theme-on-surface), 0.04);
}

.standings-table tbody tr:not(:last-child) {
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.standings-table td {
  padding: 0.55rem 0.5rem;
}

.standings-col-position {
  width: 2.5rem;
  text-align: center;
  font-weight: 600;
}

.standings-col-club {
  min-width: 220px;
}

.standings-col-stat {
  width: 3rem;
  text-align: center;
}

.standings-club-link {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  color: inherit;
  text-decoration: none;
  min-width: 0;
}

.standings-club-link:hover {
  color: rgb(var(--v-theme-primary));
  text-decoration: underline;
}

.standings-footer {
  flex: 0 0 auto;
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-surface));
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
