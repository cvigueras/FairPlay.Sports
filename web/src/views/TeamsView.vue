<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import { storeToRefs } from 'pinia'
import {
  mdiAccountGroupOutline,
  mdiArrowDown,
  mdiArrowUp,
  mdiCardAccountDetailsOutline,
  mdiChevronDown,
  mdiMagnify,
  mdiMagnifyRemoveOutline,
  mdiMapMarkerOutline,
  mdiSoccer,
  mdiSortVariant,
  mdiTrophyOutline,
  mdiTuneVariant,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useLeagueFilterStore } from '@/stores/leagueFilter'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { AGE_CATEGORIES, DIVISIONS, FOOTBALL_TYPES, type Team } from '@/types/team'
import type { PagedResult } from '@/types/pagination'

const { t } = useI18n()
const auth = useAuthStore()
const { smAndDown, xs } = useDisplay()
// xs (< 600px, Vuetify's default sm threshold) matches the CSS's own
// max-width: 599px mobile breakpoint below - smAndDown (< 960px) doesn't,
// so template logic that must switch in lockstep with that CSS uses xs.

const PAGE_SIZE = 20

const page = ref(1)
const result = ref<PagedResult<Team> | null>(null)
const loading = ref(false)
const error = ref('')

// Shared with the Standings screen, so switching pages keeps the same
// league slice in view. Always has a value - not clearable, only
// reassignable; changing one re-queries immediately.
const { type, division, category } = storeToRefs(useLeagueFilterStore())

// Free-text filters: only kick in once at least this many characters are typed.
const TEXT_FILTER_MIN_CHARS = 3
const TEXT_FILTER_DEBOUNCE_MS = 300
const nameText = ref<string | null>('')
const coachText = ref<string | null>('')
const cityText = ref<string | null>('')

const asTextFilter = (text: string | null) => {
  // The clearable "X" sets the model to null, not ''.
  const trimmed = (text ?? '').trim()
  return trimmed.length >= TEXT_FILTER_MIN_CHARS ? trimmed : undefined
}

// Sort by name/category/type/division/city. On desktop this is driven by
// clicking a column header (same mechanism as StandingsView: click to sort
// by it, click again to reverse, a new column starts descending); on
// mobile, where there's no header row to click, by the dropdown below the
// title (the app's original sort picker, now covering every column instead
// of just name/city). Both read and write the same sortField/sortDescending
// state, so switching between the two stays in sync.
const sortField = ref('name')
const sortDescending = ref(false)
const sort = computed({
  get: () => `${sortDescending.value ? '-' : ''}${sortField.value}`,
  set: (value: string) => {
    sortDescending.value = value.startsWith('-')
    sortField.value = sortDescending.value ? value.slice(1) : value
  },
})

const SORTABLE_FIELDS = ['name', 'category', 'type', 'division', 'city'] as const
const sortItems = computed(() =>
  SORTABLE_FIELDS.flatMap((field) => [
    { value: field, title: `${t(`teams.fields.${field}`)} (A-Z)` },
    { value: `-${field}`, title: `${t(`teams.fields.${field}`)} (Z-A)` },
  ]),
)

function toggleSort(field: string) {
  if (sortField.value === field) {
    sortDescending.value = !sortDescending.value
  } else {
    sortField.value = field
    sortDescending.value = true
  }
}

const hasActiveFilters = computed(
  () =>
    asTextFilter(nameText.value) !== undefined ||
    asTextFilter(coachText.value) !== undefined ||
    asTextFilter(cityText.value) !== undefined,
)

// Desktop only (see .teams-more-btn, hidden on mobile): coach/city live
// behind "More filters" there; a badge on that button surfaces how many of
// them are active without opening the menu, and a removable chip keeps them
// visible once set.
const hiddenFilterCount = computed(
  () =>
    [asTextFilter(coachText.value), asTextFilter(cityText.value)].filter(
      (value) => value !== undefined,
    ).length,
)

// Mobile only: the whole filter bar (search + the visible selects, plus the
// "more filters" menu) collapses behind a "Filters" toggle next to the sort
// select - see .teams-filterbar below. A badge on that toggle surfaces how
// many text filters are active without opening the panel.
const mobileFiltersOpen = ref(false)
const activeFilterCount = computed(
  () =>
    [asTextFilter(nameText.value), asTextFilter(coachText.value), asTextFilter(cityText.value)].filter(
      (value) => value !== undefined,
    ).length,
)

const enumItems = <T extends string>(values: readonly T[]) =>
  values.map((value) => ({ value, title: t(`profile.team.enums.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES))
const divisionItems = computed(() => enumItems(DIVISIONS))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES))

async function load() {
  loading.value = true
  error.value = ''
  try {
    result.value = await teamsApi.page(
      {
        page: page.value,
        pageSize: PAGE_SIZE,
        sort: sort.value,
        name: asTextFilter(nameText.value),
        coach: asTextFilter(coachText.value),
        city: asTextFilter(cityText.value),
        type: type.value,
        division: division.value,
        category: category.value,
      },
      auth.accessToken,
    )
  } catch (err) {
    error.value = err instanceof ApiError ? err.message : t('teams.loadFailed')
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
watch([type, division, category, sort], reload)

// Text filters are debounced so we query once the user pauses, not per keystroke.
let textFilterTimer: ReturnType<typeof setTimeout> | undefined
watch([nameText, coachText, cityText], () => {
  clearTimeout(textFilterTimer)
  textFilterTimer = setTimeout(reload, TEXT_FILTER_DEBOUNCE_MS)
})

function clearFilters() {
  nameText.value = ''
  coachText.value = ''
  cityText.value = ''
  clearTimeout(textFilterTimer)
  reload()
}

/** Which mobile rows currently have their details expanded, keyed by team id. */
const expandedTeamRows = reactive<Record<string, boolean>>({})

function toggleTeamRow(id: string) {
  expandedTeamRows[id] = !expandedTeamRows[id]
}
</script>

<template>
  <v-main>
    <div class="teams-page">
      <div class="teams-toolbar">
        <!-- The page title moved to the breadcrumb (see AppShell); this row
             now only carries the mobile sort control. -->
        <!-- Mobile only (see the max-width: 599px rules below): the table's
             sortable column headers don't exist here, so this dropdown
             (the app's original sort picker) drives the same state instead. -->
        <v-select
          v-model="sort"
          :items="sortItems"
          :prepend-inner-icon="mdiSortVariant"
          variant="outlined"
          density="comfortable"
          hide-details
          class="teams-mobile-sort"
        />
        <!-- Mobile only: reveals .teams-filterbar below, which is otherwise
             collapsed on mobile (see the max-width: 599px rules below). -->
        <v-btn
          :prepend-icon="mdiTuneVariant"
          variant="outlined"
          color="secondary"
          class="teams-filters-toggle"
          :aria-expanded="mobileFiltersOpen"
          @click="mobileFiltersOpen = !mobileFiltersOpen"
        >
          {{ t('teams.filters') }}
          <v-badge
            v-if="activeFilterCount > 0"
            :content="activeFilterCount"
            color="primary"
            inline
            class="ms-2"
          />
        </v-btn>
      </div>

      <div class="teams-filterbar" :class="{ 'teams-filterbar--open': mobileFiltersOpen }">
        <v-text-field
          v-model="nameText"
          :placeholder="t('teams.searchPlaceholder')"
          :prepend-inner-icon="mdiMagnify"
          variant="outlined"
          density="comfortable"
          hide-details
          clearable
          class="teams-search"
        />
        <v-select
          v-model="type"
          :items="typeItems"
          :label="t('teams.fields.type')"
          variant="outlined"
          density="comfortable"
          hide-details
          class="teams-filter-select"
        />
        <v-select
          v-model="division"
          :items="divisionItems"
          :label="t('teams.fields.division')"
          variant="outlined"
          density="comfortable"
          hide-details
          class="teams-filter-select"
        />
        <v-select
          v-model="category"
          :items="categoryItems"
          :label="t('teams.fields.category')"
          variant="outlined"
          density="comfortable"
          hide-details
          class="teams-filter-select"
        />
        <!-- Mobile only: coach/city live behind "More filters" on desktop
             (see .teams-more-btn below), but that nested menu is one tap too
             many on top of the filter panel toggle, so on mobile they're
             flattened in here instead - same size as every other selector.
             v-if (not CSS) keeps these out of the desktop DOM entirely, so
             there are never two coach/city fields fighting over one model. -->
        <v-text-field
          v-if="xs"
          v-model="coachText"
          :label="t('teams.fields.coach')"
          variant="outlined"
          density="comfortable"
          hide-details
          clearable
          class="teams-filter-select"
        />
        <v-text-field
          v-if="xs"
          v-model="cityText"
          :label="t('teams.fields.city')"
          variant="outlined"
          density="comfortable"
          hide-details
          clearable
          class="teams-filter-select"
        />
        <v-menu :close-on-content-click="false" location="bottom end">
          <template #activator="{ props: menuProps }">
            <v-btn
              v-bind="menuProps"
              :prepend-icon="mdiTuneVariant"
              variant="outlined"
              color="secondary"
              class="teams-more-btn"
            >
              {{ t('teams.moreFilters') }}
              <v-badge
                v-if="hiddenFilterCount > 0"
                :content="hiddenFilterCount"
                color="primary"
                inline
                class="ms-2"
              />
            </v-btn>
          </template>
          <v-card min-width="260" class="pa-4 d-flex flex-column ga-4">
            <v-text-field
              v-model="coachText"
              :label="t('teams.fields.coach')"
              variant="outlined"
              density="comfortable"
              hide-details
              clearable
            />
            <v-text-field
              v-model="cityText"
              :label="t('teams.fields.city')"
              variant="outlined"
              density="comfortable"
              hide-details
              clearable
            />
          </v-card>
        </v-menu>
      </div>

      <div v-if="hasActiveFilters || xs" class="teams-active-chips">
        <!-- Mobile only: type/division/category sit inside the collapsed
             filter panel (see .teams-filterbar below) same as everything
             else, so without these the list would filter silently - no
             visible sign of which segment is applied. Not closable: like
             the selects themselves, these are always set, never cleared. -->
        <template v-if="xs">
          <v-chip size="small" variant="tonal" :color="MODALITY_COLOR[type]" :prepend-icon="mdiSoccer">
            {{ t(`profile.team.enums.${type}`) }}
          </v-chip>
          <v-chip size="small" variant="tonal" :color="DIVISION_COLOR[division]" :prepend-icon="mdiTrophyOutline">
            {{ t(`profile.team.enums.${division}`) }}
          </v-chip>
          <v-chip size="small" variant="tonal" :color="AGE_CATEGORY_COLOR[category]">
            {{ t(`profile.team.enums.${category}`) }}
          </v-chip>
        </template>
        <!-- On mobile the search box lives inside the collapsed filter panel
             (see .teams-filterbar below), so this chip is the only sign a
             name filter is active while the panel stays closed. -->
        <v-chip
          v-if="asTextFilter(nameText) !== undefined"
          size="small"
          variant="tonal"
          closable
          @click:close="nameText = ''"
        >
          {{ t('teams.fields.name') }}: {{ nameText }}
        </v-chip>
        <v-chip
          v-if="asTextFilter(coachText) !== undefined"
          size="small"
          variant="tonal"
          closable
          @click:close="coachText = ''"
        >
          {{ t('teams.fields.coach') }}: {{ coachText }}
        </v-chip>
        <v-chip
          v-if="asTextFilter(cityText) !== undefined"
          size="small"
          variant="tonal"
          closable
          @click:close="cityText = ''"
        >
          {{ t('teams.fields.city') }}: {{ cityText }}
        </v-chip>
        <v-btn
          v-if="hasActiveFilters"
          variant="text"
          size="small"
          color="error"
          @click="clearFilters"
        >
          {{ t('teams.clearFilters') }}
        </v-btn>
      </div>

      <div class="teams-list">
        <v-progress-circular
          v-if="loading"
          indeterminate
          color="primary"
          class="d-block mx-auto my-16"
        />
        <v-alert v-else-if="error" type="error" variant="tonal">{{ error }}</v-alert>

        <template v-else-if="result">
          <div
            v-if="result.items.length === 0"
            class="teams-empty text-medium-emphasis"
          >
            <v-icon
              :icon="hasActiveFilters ? mdiMagnifyRemoveOutline : mdiAccountGroupOutline"
              size="48"
            />
            <p class="text-body-1 mt-3">
              {{ hasActiveFilters ? t('teams.noResults') : t('teams.empty') }}
            </p>
          </div>

          <template v-else>
          <div class="teams-table-wrap">
            <table class="teams-table">
              <colgroup>
                <col class="teams-col-club">
                <col class="teams-col-stat">
                <col class="teams-col-stat">
                <col class="teams-col-stat">
                <col class="teams-col-city">
                <col class="teams-col-actions">
              </colgroup>
              <thead>
                <tr>
                  <th
                    class="teams-col-club teams-col-sortable"
                    :class="{ 'teams-col-sortable--active': sortField === 'name' }"
                    @click="toggleSort('name')"
                  >
                    <span class="teams-th-inner">
                      {{ t('teams.fields.name') }}
                      <v-icon
                        v-if="sortField === 'name'"
                        :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                        size="14"
                      />
                    </span>
                  </th>
                  <th
                    class="teams-col-stat teams-col-sortable"
                    :class="{ 'teams-col-sortable--active': sortField === 'category' }"
                    @click="toggleSort('category')"
                  >
                    <span class="teams-th-inner">
                      {{ t('teams.fields.category') }}
                      <v-icon
                        v-if="sortField === 'category'"
                        :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                        size="14"
                      />
                    </span>
                  </th>
                  <th
                    class="teams-col-stat teams-col-sortable"
                    :class="{ 'teams-col-sortable--active': sortField === 'type' }"
                    @click="toggleSort('type')"
                  >
                    <span class="teams-th-inner">
                      {{ t('teams.fields.type') }}
                      <v-icon
                        v-if="sortField === 'type'"
                        :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                        size="14"
                      />
                    </span>
                  </th>
                  <th
                    class="teams-col-stat teams-col-sortable"
                    :class="{ 'teams-col-sortable--active': sortField === 'division' }"
                    @click="toggleSort('division')"
                  >
                    <span class="teams-th-inner">
                      {{ t('teams.fields.division') }}
                      <v-icon
                        v-if="sortField === 'division'"
                        :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                        size="14"
                      />
                    </span>
                  </th>
                  <th
                    class="teams-col-city teams-col-sortable"
                    :class="{ 'teams-col-sortable--active': sortField === 'city' }"
                    @click="toggleSort('city')"
                  >
                    <span class="teams-th-inner">
                      {{ t('teams.fields.city') }}
                      <v-icon
                        v-if="sortField === 'city'"
                        :icon="sortDescending ? mdiArrowDown : mdiArrowUp"
                        size="14"
                      />
                    </span>
                  </th>
                  <th class="teams-col-actions"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="team in result.items" :key="team.id">
                  <td class="teams-col-club">
                    <div class="team-row-club">
                      <TeamCrest :team="team" :size="32" />
                      <span class="text-truncate">{{ team.name }}</span>
                    </div>
                  </td>
                  <td class="teams-col-stat">
                    <v-chip size="small" variant="tonal" :color="AGE_CATEGORY_COLOR[team.category]">
                      {{ t(`profile.team.enums.${team.category}`) }}
                    </v-chip>
                  </td>
                  <td class="teams-col-stat">
                    <v-chip
                      size="small"
                      variant="tonal"
                      :color="MODALITY_COLOR[team.type]"
                      :prepend-icon="mdiSoccer"
                    >
                      {{ t(`profile.team.enums.${team.type}`) }}
                    </v-chip>
                  </td>
                  <td class="teams-col-stat">
                    <v-chip
                      v-if="team.division"
                      size="small"
                      variant="tonal"
                      :color="DIVISION_COLOR[team.division]"
                      :prepend-icon="mdiTrophyOutline"
                    >
                      {{ t(`profile.team.enums.${team.division}`) }}
                    </v-chip>
                    <span v-else class="text-body-2 text-medium-emphasis">—</span>
                  </td>
                  <td class="teams-col-city">
                    <span class="team-row-city text-body-2 text-medium-emphasis">
                      <v-icon size="14" :icon="mdiMapMarkerOutline" />
                      {{ team.city }}
                    </span>
                  </td>
                  <td class="teams-col-actions">
                    <v-btn
                      :to="{ name: 'team-detail', params: { id: team.id } }"
                      :prepend-icon="mdiCardAccountDetailsOutline"
                      color="blue"
                      variant="outlined"
                      size="small"
                    >
                      {{ t('profile.team.viewDetails') }}
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Mobile only (see the max-width: 599px rules below): one row
               per team - crest, name, category - that expands in place
               instead of the desktop table's horizontal scroll. -->
          <div class="teams-mobile">
            <div class="teams-mobile-card">
              <template v-for="team in result.items" :key="team.id">
                <button
                  type="button"
                  class="teams-mobile-row"
                  :aria-expanded="!!expandedTeamRows[team.id]"
                  @click="toggleTeamRow(team.id)"
                >
                  <TeamCrest :team="team" :size="28" />
                  <span class="teams-mobile-name text-truncate">{{ team.name }}</span>
                  <v-chip size="small" variant="tonal" :color="AGE_CATEGORY_COLOR[team.category]">
                    {{ t(`profile.team.enums.${team.category}`) }}
                  </v-chip>
                  <v-icon
                    :icon="mdiChevronDown"
                    size="18"
                    class="teams-mobile-chevron"
                    :class="{ 'teams-mobile-chevron--open': expandedTeamRows[team.id] }"
                  />
                </button>

                <div v-if="expandedTeamRows[team.id]" class="teams-mobile-details">
                  <div class="teams-mobile-detail-grid">
                    <div class="teams-mobile-detail-cell">
                      <span class="teams-mobile-detail-label">{{ t('teams.fields.type') }}</span>
                      <v-chip size="small" variant="tonal" :color="MODALITY_COLOR[team.type]" :prepend-icon="mdiSoccer">
                        {{ t(`profile.team.enums.${team.type}`) }}
                      </v-chip>
                    </div>
                    <div class="teams-mobile-detail-cell">
                      <span class="teams-mobile-detail-label">{{ t('teams.fields.division') }}</span>
                      <v-chip
                        v-if="team.division"
                        size="small"
                        variant="tonal"
                        :color="DIVISION_COLOR[team.division]"
                        :prepend-icon="mdiTrophyOutline"
                      >
                        {{ t(`profile.team.enums.${team.division}`) }}
                      </v-chip>
                      <span v-else class="text-body-2 text-medium-emphasis">—</span>
                    </div>
                  </div>
                  <div class="teams-mobile-detail-cell">
                    <span class="teams-mobile-detail-label">{{ t('teams.fields.city') }}</span>
                    <span class="teams-mobile-city">
                      <v-icon size="14" :icon="mdiMapMarkerOutline" />
                      {{ team.city }}
                    </span>
                  </div>
                  <v-btn
                    :to="{ name: 'team-detail', params: { id: team.id } }"
                    :prepend-icon="mdiCardAccountDetailsOutline"
                    color="blue"
                    variant="outlined"
                    block
                  >
                    {{ t('profile.team.viewDetails') }}
                  </v-btn>
                </div>
              </template>
            </div>
          </div>
          </template>
        </template>
      </div>

      <footer v-if="result" class="teams-footer">
        <div class="teams-footer-inner">
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
          <p class="teams-count font-weight-bold">
            {{ t('teams.count', { n: result.totalCount }) }}
          </p>
        </div>
      </footer>
    </div>
  </v-main>
</template>

<style scoped>
/* Fill the space under the app bar; only the list scrolls, header and pager stay put. */
.teams-page {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: calc(100dvh - var(--v-layout-top, 64px));
  overflow: hidden;
  padding-top: 1.5rem;
}

/* The page spans the whole content area so the footer rule runs edge to edge;
   the actual content stays capped and centred. */
.teams-toolbar,
.teams-filterbar,
.teams-active-chips,
.teams-list,
.teams-footer-inner {
  width: 100%;
  max-width: 1600px;
  margin-inline: auto;
  padding-inline: 1.5rem;
}

.teams-toolbar {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
  padding-bottom: 0.75rem;
}

/* Desktop sorts via the table's column headers (see .teams-col-sortable
   below); this stays hidden until the max-width: 599px rules turn it on
   for the mobile list, which has no header row to click. */
.teams-mobile-sort {
  display: none;
}

/* Desktop shows every filter inline in .teams-filterbar already; this toggle
   only exists to collapse that bar behind a button on mobile (see the
   max-width: 599px rules below). */
.teams-filters-toggle {
  display: none;
}

.teams-filterbar {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
  padding-bottom: 0.75rem;
}

.teams-search {
  flex: 1 1 240px;
  min-width: 200px;
}

.teams-filter-select {
  flex: 1 1 160px;
  max-width: 220px;
}

.teams-more-btn {
  flex: 0 0 auto;
}

.teams-active-chips {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  padding-bottom: 1rem;
}

.teams-list {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding-bottom: 1rem;
  /* The table below can be wider than the viewport on small screens, so this
     same element also scrolls horizontally (see .teams-table-wrap). Keep the
     vertical scrollbar hidden (it's the page's main scroll, always
     available) but show a slim horizontal one, as in StandingsView. */
  scrollbar-width: thin;
  -ms-overflow-style: none;
  -webkit-overflow-scrolling: touch;
}

.teams-list::-webkit-scrollbar {
  width: 0;
  height: 6px;
}

.teams-list::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.teams-list::-webkit-scrollbar-track {
  background: transparent;
}

.teams-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 4rem 1rem;
}

.teams-footer {
  flex: 0 0 auto;
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  /* Same colour as the app bar (Vuetify toolbars default to `surface`). */
  background: rgb(var(--v-theme-surface));
}

.teams-footer-inner {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 3rem;
  padding-block: 0.75rem 1rem;
}

/* Pinned to the far right, on the same line as the (centred) pager. */
.teams-count {
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

/* Not enough room to pin the count beside the pager - stack them instead. */
@media (max-width: 599px) {
  .teams-footer-inner {
    flex-direction: column;
    gap: 0.35rem;
  }

  .teams-count {
    position: static;
  }

  .teams-footer-inner :deep(.v-pagination) {
    margin-inline: 1.5rem;
  }
}

/* One team per line: a compact table (same shape as StandingsView's),
   wrapped in a bordered card so the list reads as one block. No overflow
   here (see the .teams-list comment above) - the rounded corners are cut
   into the table's own corner cells instead. */
.teams-table-wrap {
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 16px;
}

.teams-table {
  width: 100%;
  min-width: 900px;
  border-collapse: collapse;
  table-layout: fixed;
  font-size: 0.875rem;
}

.teams-col-club {
  width: 25%;
}

.teams-col-stat {
  width: 14%;
}

.teams-col-city {
  width: 13%;
}

/* Wide enough that the icon + label never get squeezed inside the button,
   even at the table's min-width (see .teams-table above). */
.teams-col-actions {
  width: 20%;
}

.teams-table thead th {
  position: sticky;
  top: 0;
  z-index: 2;
  background: rgb(var(--v-theme-background));
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  padding: 0.65rem 0.75rem;
  text-align: left;
  font-size: 0.875rem;
  font-weight: 700;
  color: #64748b;
  white-space: nowrap;
  user-select: none;
}

.teams-table thead th.teams-col-sortable {
  cursor: pointer;
}

.teams-table thead th.teams-col-sortable--active {
  color: #15803d;
}

.teams-th-inner {
  display: inline-flex;
  align-items: center;
  gap: 0.15rem;
}

.teams-table thead th:first-child {
  border-top-left-radius: 16px;
}

.teams-table thead th:last-child {
  border-top-right-radius: 16px;
}

.teams-table tbody tr:last-child td:first-child {
  border-bottom-left-radius: 16px;
}

.teams-table tbody tr:last-child td:last-child {
  border-bottom-right-radius: 16px;
}

.teams-table tbody tr:not(:last-child) td {
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.teams-table td {
  padding: 0.5rem 0.75rem;
  vertical-align: middle;
  text-align: center;
}

.teams-table td.teams-col-club,
.teams-table td.teams-col-city {
  text-align: left;
}

.teams-table td.teams-col-actions {
  text-align: right;
}

/* Age category, modality and division chips fill their (equal-width)
   column so the three read as same-sized labels, whatever their text. */
.teams-table td.teams-col-stat :deep(.v-chip) {
  width: 100%;
  justify-content: center;
}

.team-row-club {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  min-width: 0;
  font-weight: 700;
}

.team-row-club span {
  min-width: 0;
}

.team-row-city {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

/* Mobile-only replacement for the desktop table (see the max-width: 599px
   rules below, which swap the two). One row per team - crest, name,
   category - that expands in place instead of scrolling sideways. */
.teams-mobile {
  display: none;
}

.teams-mobile-card {
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 16px;
  overflow: hidden;
}

.teams-mobile-card > *:last-child {
  border-bottom: none;
}

.teams-mobile-row {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.65rem 0.85rem;
  border: none;
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-surface));
  font: inherit;
  text-align: left;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
  min-height: 44px;
}

.teams-mobile-name {
  flex: 1;
  min-width: 0;
  font-weight: 600;
  font-size: 0.9375rem;
  color: #0f172a;
}

.teams-mobile-chevron {
  flex-shrink: 0;
  color: #94a3b8;
  transition: transform 0.15s ease;
}

.teams-mobile-chevron--open {
  transform: rotate(180deg);
}

.teams-mobile-details {
  padding: 0.75rem 0.85rem 0.9rem;
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-background));
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
}

.teams-mobile-detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.6rem;
}

.teams-mobile-detail-cell {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.teams-mobile-detail-label {
  font-size: 0.6875rem;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.teams-mobile-detail-cell :deep(.v-chip) {
  width: fit-content;
}

.teams-mobile-city {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  color: #475569;
  font-size: 0.8125rem;
  font-weight: 600;
}

@media (max-width: 599px) {
  .teams-table-wrap {
    display: none;
  }

  .teams-mobile {
    display: block;
  }

  .teams-mobile-sort {
    display: block;
    flex: 1 1 200px;
  }

  .teams-filters-toggle {
    display: inline-flex;
    flex: 0 0 auto;
  }

  /* Collapsed by default (see .teams-filters-toggle above); opening it
     stacks every filter - search, the three selects and coach/city - as one
     column of equal-width controls instead of desktop's wrapping row. */
  .teams-filterbar {
    display: none;
  }

  .teams-filterbar--open {
    display: flex;
    flex-direction: column;
    align-items: stretch;
  }

  .teams-filterbar--open .teams-search,
  .teams-filterbar--open .teams-filter-select {
    flex: 1 1 auto;
    max-width: none;
    width: 100%;
  }

  /* Coach/city are flattened straight into the panel above on mobile
     instead (see the v-if="xs" fields in the template) - stacking this on
     top would be a menu inside an already-collapsible panel, one tap too
     many. */
  .teams-more-btn {
    display: none;
  }
}
</style>
