<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAccountGroupOutline,
  mdiAccountOutline,
  mdiClipboardTextOutline,
  mdiFilterRemoveOutline,
  mdiFilterVariant,
  mdiMagnifyRemoveOutline,
  mdiMapMarkerOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiSwordCross,
  mdiTrophyOutline,
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

// Filters start collapsed on every viewport; a button reveals them.
const filtersOpen = ref(false)

const hasActiveFilters = computed(
  () =>
    asTextFilter(nameText.value) !== undefined ||
    asTextFilter(coachText.value) !== undefined ||
    asTextFilter(cityText.value) !== undefined ||
    type.value != null ||
    division.value != null ||
    category.value != null,
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
        sort: 'name',
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

// A filter change goes back to the first page; reload directly if already there.
function reload() {
  if (page.value === 1) load()
  else page.value = 1
}

watch(page, load, { immediate: true })
watch([type, division, category], reload)

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
  filtersOpen.value = false
  clearTimeout(textFilterTimer)
  reload()
}

/** The team whose full sheet is open in the modal, or null when closed. */
const sheetTeam = ref<Team | null>(null)

function challengeTeam(_team: Team) {
  // TODO: wire up the team-vs-team challenge flow.
}
</script>

<template>
  <v-main>
    <div class="teams-page">
      <div class="teams-filters-bar">
        <v-btn
          :prepend-icon="mdiFilterVariant"
          :append-icon="filtersOpen ? '$collapse' : '$expand'"
          variant="tonal"
          size="small"
          @click="filtersOpen = !filtersOpen"
        >
          {{ t('teams.filters') }}
          <v-badge v-if="hasActiveFilters" color="primary" dot inline class="ms-2" />
        </v-btn>
        <v-btn
          :prepend-icon="mdiFilterRemoveOutline"
          :disabled="!hasActiveFilters"
          color="error"
          variant="tonal"
          size="small"
          @click="clearFilters"
        >
          {{ t('teams.clearFilters') }}
        </v-btn>
      </div>

      <v-expand-transition>
        <div v-show="filtersOpen" class="teams-filters">
          <v-text-field
            v-model="nameText"
            :label="t('teams.fields.name')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
          />
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
          <v-select
            v-model="type"
            :items="typeItems"
            :label="t('teams.fields.type')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
          />
          <v-select
            v-model="division"
            :items="divisionItems"
            :label="t('teams.fields.division')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
          />
          <v-select
            v-model="category"
            :items="categoryItems"
            :label="t('teams.fields.category')"
            variant="outlined"
            density="comfortable"
            hide-details
            clearable
          />
        </div>
      </v-expand-transition>

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

          <div class="teams-grid">
            <v-card
              v-for="team in result.items"
              :key="team.id"
              border
              flat
              rounded="xl"
              class="bkt bkt2 px-4 py-3 px-md-6"
            >
              <div class="bkt-crest">
                <TeamCrest :team="team" :size="76" />
              </div>
              <div class="bkt-body">
                <div class="bkt-row1">
                  <span class="text-subtitle-1 font-weight-bold">{{ team.name }}</span>
                  <span class="text-disabled">·</span>
                  <span
                    class="text-subtitle-1 font-weight-bold"
                    :style="{ color: AGE_CATEGORY_COLOR[team.category] }"
                  >
                    {{ t(`profile.team.enums.${team.category}`) }}
                  </span>
                </div>
                <div class="bkt-chips">
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
                <div class="bkt-meta text-body-2 text-medium-emphasis">
                  <span>
                    <v-icon size="14" :icon="mdiAccountOutline" color="#5D4037" />
                    {{ team.coach }}
                  </span>
                  <span v-if="team.venueName">
                    <v-icon size="14" :icon="mdiSoccerField" color="#2E7D32" />
                    {{ team.venueName }}
                  </span>
                </div>
              </div>
              <div class="bkt-actions">
                <v-btn
                  color="red"
                  variant="outlined"
                  size="small"
                  :prepend-icon="mdiSwordCross"
                  @click="challengeTeam(team)"
                >
                  {{ t('profile.team.challenge') }}
                </v-btn>
                <v-btn
                  color="blue"
                  variant="outlined"
                  size="small"
                  :prepend-icon="mdiClipboardTextOutline"
                  @click="sheetTeam = team"
                >
                  {{ t('profile.team.viewSheet') }}
                </v-btn>
              </div>
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

    <!-- Full club-sheet modal ("view sheet" action) -->
    <v-dialog
      :model-value="sheetTeam !== null"
      max-width="420"
      @update:model-value="sheetTeam = null"
    >
      <v-card v-if="sheetTeam" border flat rounded="xl" class="pa-6">
        <h3 class="text-h6 font-weight-bold mb-3">
          {{ sheetTeam.name }}<span v-if="sheetTeam.shortName"> · {{ sheetTeam.shortName }}</span>
        </h3>
        <dl class="team-info-dl">
          <dt>{{ t('profile.team.category') }}</dt>
          <dd>{{ t(`profile.team.enums.${sheetTeam.category}`) }}</dd>
          <dt>{{ t('profile.team.type') }}</dt>
          <dd>{{ t(`profile.team.enums.${sheetTeam.type}`) }}</dd>
          <dt>{{ t('profile.team.division') }}</dt>
          <dd>{{ t(`profile.team.enums.${sheetTeam.division}`) }}</dd>
          <dt>{{ t('profile.team.city') }}</dt>
          <dd>{{ sheetTeam.city }}</dd>
          <dt>{{ t('profile.team.coach') }}</dt>
          <dd>{{ sheetTeam.coach }}</dd>
          <template v-if="sheetTeam.foundedYear">
            <dt>{{ t('profile.team.foundedYear') }}</dt>
            <dd>{{ sheetTeam.foundedYear }}</dd>
          </template>
          <template v-if="sheetTeam.venueName">
            <dt>{{ t('profile.team.venueGroup') }}</dt>
            <dd>{{ sheetTeam.venueName }}</dd>
          </template>
          <template v-if="sheetTeam.colorPrimary && sheetTeam.colorSecondary">
            <dt>{{ t('profile.team.colorsGroup') }}</dt>
            <dd>{{ sheetTeam.colorPrimary }} / {{ sheetTeam.colorSecondary }}</dd>
          </template>
          <template v-if="sheetTeam.contactEmail">
            <dt>{{ t('profile.team.contactEmail') }}</dt>
            <dd>{{ sheetTeam.contactEmail }}</dd>
          </template>
          <template v-if="sheetTeam.contactPhone">
            <dt>{{ t('profile.team.contactPhone') }}</dt>
            <dd>{{ sheetTeam.contactPhone }}</dd>
          </template>
          <template v-if="sheetTeam.website">
            <dt>{{ t('profile.team.website') }}</dt>
            <dd>{{ sheetTeam.website }}</dd>
          </template>
        </dl>

        <div class="d-flex justify-end mt-4">
          <v-btn variant="text" @click="sheetTeam = null">{{ t('common.close') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>
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
.teams-filters-bar,
.teams-filters,
.teams-list,
.teams-footer-inner {
  width: 100%;
  max-width: 1600px;
  margin-inline: auto;
  padding-inline: 1.5rem;
}

.teams-filters-bar {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding-bottom: 0.75rem;
}

.teams-filters {
  flex: 0 0 auto;
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
  padding-bottom: 1rem;
}

@media (min-width: 600px) {
  .teams-filters {
    grid-template-columns: repeat(3, 1fr);
  }
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

.teams-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

/* Club panel — shared visual language with the "My teams" screen: crest on
   the left, name + age category, colour-coded chips, coach / venue line, and
   the challenge / view-sheet actions stacked on the right. */
.bkt {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  overflow: hidden;
}
.bkt-crest {
  flex-shrink: 0;
}
.bkt2 .bkt-crest {
  margin-right: 0.75rem;
}
/* Slightly larger chip text without growing the fixed x-small chip height. */
.bkt2 .bkt-chips :deep(.v-chip) {
  font-size: 0.75rem;
}
.bkt-body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.bkt-row1 {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.bkt-chips {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
}
.bkt-meta {
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
}
.bkt-meta span {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  min-width: 0;
}
.bkt-actions {
  flex-shrink: 0;
  align-self: center;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* Phones: let the actions drop below the body as a full-width row. */
@media (max-width: 599px) {
  .bkt {
    flex-wrap: wrap;
  }
  .bkt-actions {
    flex-direction: row;
    width: 100%;
  }
  .bkt-actions :deep(.v-btn) {
    flex: 1;
  }
}

.team-info-dl {
  display: grid;
  grid-template-columns: auto 1fr;
  column-gap: 1rem;
  row-gap: 0.35rem;
  margin: 0;
}

.team-info-dl dt {
  font-size: 0.75rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-info-dl dd {
  margin: 0;
  font-weight: 500;
}
</style>
