<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAccountGroupOutline,
  mdiAccountOutline,
  mdiCardAccountDetailsOutline,
  mdiMagnify,
  mdiMagnifyRemoveOutline,
  mdiMapMarkerOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiSortVariant,
  mdiTrophyOutline,
  mdiTuneVariant,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  type AgeCategory,
  type Division,
  type FootballType,
  type Team,
} from '@/types/team'
import type { PagedResult } from '@/types/pagination'

const { t } = useI18n()
const auth = useAuthStore()
const { smAndDown } = useDisplay()

const PAGE_SIZE = 20

const page = ref(1)
const result = ref<PagedResult<Team> | null>(null)
const loading = ref(false)
const error = ref('')

// Dropdown filters: empty (null) means "no filter"; changing one re-queries immediately.
const type = ref<FootballType | null>(null)
const division = ref<Division | null>(null)
const category = ref<AgeCategory | null>(null)

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

const sort = ref('name')
const sortItems = computed(() => [
  { value: 'name', title: t('teams.sort.nameAsc') },
  { value: '-name', title: t('teams.sort.nameDesc') },
  { value: 'city', title: t('teams.sort.cityAsc') },
  { value: '-createdAt', title: t('teams.sort.recent') },
])

const hasActiveFilters = computed(
  () =>
    asTextFilter(nameText.value) !== undefined ||
    asTextFilter(coachText.value) !== undefined ||
    asTextFilter(cityText.value) !== undefined ||
    type.value != null ||
    division.value != null ||
    category.value != null,
)

// Coach/city live behind "More filters"; a badge on that button surfaces how
// many of them are active without opening the menu, and a removable chip
// keeps them visible once set.
const hiddenFilterCount = computed(
  () =>
    [asTextFilter(coachText.value), asTextFilter(cityText.value)].filter(
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
        type: type.value ?? undefined,
        division: division.value ?? undefined,
        category: category.value ?? undefined,
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
  type.value = null
  division.value = null
  category.value = null
  clearTimeout(textFilterTimer)
  reload()
}
</script>

<template>
  <v-main>
    <div class="teams-page">
      <div class="teams-toolbar">
        <h1 class="text-h5 font-weight-bold d-flex align-center ga-2">
          <v-icon :icon="mdiAccountGroupOutline" color="primary" />
          {{ t('teams.title') }}
        </h1>
        <v-select
          v-model="sort"
          :items="sortItems"
          :prepend-inner-icon="mdiSortVariant"
          variant="outlined"
          density="comfortable"
          hide-details
          class="teams-sort"
        />
      </div>

      <div class="teams-filterbar">
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
          clearable
          class="teams-filter-select"
        />
        <v-select
          v-model="division"
          :items="divisionItems"
          :label="t('teams.fields.division')"
          variant="outlined"
          density="comfortable"
          hide-details
          clearable
          class="teams-filter-select"
        />
        <v-select
          v-model="category"
          :items="categoryItems"
          :label="t('teams.fields.category')"
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

      <div v-if="hasActiveFilters" class="teams-active-chips">
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
        <v-btn variant="text" size="small" color="error" @click="clearFilters">
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

          <div v-else class="teams-card-grid">
            <v-card
              v-for="team in result.items"
              :key="team.id"
              border
              flat
              rounded="xl"
              class="team-card"
            >
              <div class="team-card-head">
                <TeamCrest :team="team" :size="60" />
                <div class="team-card-title">
                  <div class="text-subtitle-1 font-weight-bold text-truncate">{{ team.name }}</div>
                  <v-chip
                    size="small"
                    variant="tonal"
                    :color="AGE_CATEGORY_COLOR[team.category]"
                    class="team-card-category"
                  >
                    {{ t(`profile.team.enums.${team.category}`) }}
                  </v-chip>
                </div>
              </div>

              <div class="team-card-chips">
                <v-chip
                  size="x-small"
                  variant="tonal"
                  :color="MODALITY_COLOR[team.type]"
                  :prepend-icon="mdiSoccer"
                >
                  {{ t(`profile.team.enums.${team.type}`) }}
                </v-chip>
                <v-chip
                  size="x-small"
                  variant="tonal"
                  :color="DIVISION_COLOR[team.division]"
                  :prepend-icon="mdiTrophyOutline"
                >
                  {{ t(`profile.team.enums.${team.division}`) }}
                </v-chip>
                <v-chip size="x-small" variant="tonal" :prepend-icon="mdiMapMarkerOutline">
                  {{ team.city }}
                </v-chip>
              </div>

              <v-divider />

              <div class="team-card-meta text-body-2 text-medium-emphasis">
                <span class="team-card-meta-row">
                  <v-icon size="14" :icon="mdiAccountOutline" color="#5D4037" />
                  <span class="team-card-meta-text">{{ team.coach }}</span>
                </span>
                <span v-if="team.venueName" class="team-card-meta-row">
                  <v-icon size="14" :icon="mdiSoccerField" color="#2E7D32" />
                  <span class="team-card-meta-text">{{ team.venueName }}</span>
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
            </v-card>
          </div>
        </template>
      </div>

      <footer v-if="result" class="teams-footer">
        <div class="teams-footer-inner">
          <v-pagination
            v-if="result.totalPages > 1"
            v-model="page"
            :length="result.totalPages"
            :total-visible="smAndDown ? 3 : 7"
            rounded="circle"
            density="comfortable"
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

.teams-sort {
  flex: 0 1 220px;
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
  /* Scrollable, but the scrollbar itself is hidden. */
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.teams-list::-webkit-scrollbar {
  width: 0;
  height: 0;
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

/* Responsive card grid: as many 300px+ columns as fit, one on a phone. */
.teams-card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.25rem;
  padding-bottom: 0.5rem;
}

.team-card {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.team-card-head {
  display: flex;
  align-items: flex-start;
  gap: 0.85rem;
}

.team-card-title {
  min-width: 0;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.team-card-category {
  align-self: flex-start;
}

.team-card-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}

.team-card-meta {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.team-card-meta-row {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  min-width: 0;
}

.team-card-meta-text {
  display: block;
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
