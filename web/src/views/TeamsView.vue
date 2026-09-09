<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiAccountGroupOutline, mdiMagnifyRemoveOutline, mdiAccountTieOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import TeamCrest from '@/components/TeamCrest.vue'
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

const { t, locale } = useI18n()
const auth = useAuthStore()

const PAGE_SIZE = 12

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

const formatLongDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'long', day: 'numeric' })

/** Fields laid out in a three-column grid (name and coach show by the crest). */
function fields(team: Team) {
  return [
    { label: t('teams.fields.city'), value: team.city },
    { label: t('teams.fields.type'), value: t(`profile.team.enums.${team.type}`) },
    { label: t('teams.fields.division'), value: t(`profile.team.enums.${team.division}`) },
    { label: t('teams.fields.category'), value: t(`profile.team.enums.${team.category}`) },
  ]
}
</script>

<template>
  <v-main>
    <div class="teams-page">
      <div class="teams-filters">
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
            class="team-card pa-4 pa-sm-6"
          >
            <div class="team-crest-col">
              <div class="team-name text-h6 font-weight-bold">{{ team.name }}</div>
              <div class="team-crest-row">
                <TeamCrest :team="team" :size="72" />
                <div class="member-since">
                  <div class="member-since-label text-medium-emphasis">
                    {{ t('teams.fields.memberSince') }}
                  </div>
                  <div class="member-since-bar"></div>
                  <div class="member-since-value font-weight-medium">
                    {{ formatLongDate(team.createdAt) }}
                  </div>
                </div>
              </div>
              <div class="team-coach">
                <v-icon :icon="mdiAccountTieOutline" size="18" class="team-coach-icon" />
                <span class="text-body-2 font-weight-medium">{{ team.coach }}</span>
                <v-tooltip activator="parent" location="top" :text="t('teams.fields.coach')" />
              </div>
              <v-divider class="team-coach-rule" />
            </div>

            <div class="team-fields">
              <div v-for="field in fields(team)" :key="field.label" class="team-field">
                <div class="text-caption text-medium-emphasis">{{ field.label }}</div>
                <v-divider class="my-1" />
                <div class="text-body-1 font-weight-medium">{{ field.value }}</div>
              </div>
            </div>
          </v-card>
          </div>
        </template>
      </div>

      <footer v-if="result" class="teams-footer">
        <v-pagination
          v-if="result.totalPages > 1"
          v-model="page"
          :length="result.totalPages"
          :total-visible="7"
          rounded="circle"
          density="comfortable"
        />
        <p class="teams-count font-weight-bold">
          {{ t('teams.count', { n: result.totalCount }) }}
        </p>
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
  max-width: 1200px;
  margin-inline: auto;
  height: calc(100dvh - var(--v-layout-top, 64px));
  overflow: hidden;
  padding: 1.5rem 1.5rem 0;
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
}

.teams-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 4rem 1rem;
}

.teams-footer {
  position: relative;
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 3rem;
  padding: 0.75rem 0 1rem;
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-background));
}

/* Pinned to the far right, on the same line as the (centred) pager. */
.teams-count {
  position: absolute;
  right: 0;
  margin: 0;
}

.teams-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

.team-card {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.team-crest-col {
  flex: 0 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  text-align: center;
  /* Very light blue panel that bleeds to the card edges, down to the rule. */
  margin: -1rem -1rem 0;
  padding: 1rem 1rem 0;
  background: rgba(59, 130, 246, 0.04);
}

.team-name {
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.team-crest-row {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.team-coach {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  min-width: 0;
}

.team-coach-icon {
  flex: 0 0 auto;
  opacity: 0.7;
}

/* Full-bleed: cancel the card's padding (pa-4 / pa-sm-6) so it meets both edges. */
.team-coach-rule {
  align-self: stretch;
  width: auto;
  margin-inline: -1rem;
}

@media (min-width: 600px) {
  .team-coach-rule {
    margin-inline: -1.5rem;
  }
}

.member-since {
  min-width: 0;
}

.member-since-label {
  font-size: 0.68rem;
  line-height: 1.2;
}

.member-since-bar {
  width: 100%;
  height: 3px;
  margin: 4px 0;
  border-radius: 2px;
  background: #86efac;
}

.member-since-value {
  font-size: 0.75rem;
  line-height: 1.25;
}

.team-fields {
  flex: 1 1 auto;
  min-width: 0;
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem 2rem;
  align-content: center;
}

.team-field {
  min-width: 0;
}

@media (min-width: 600px) {
  .team-card {
    flex-direction: row;
    align-items: center;
  }

  .team-crest-col {
    width: 260px;
    margin: -1.5rem 0 -1.5rem -1.5rem;
    padding: 1.5rem;
  }
}

@media (min-width: 860px) {
  .team-fields {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* Multiple teams per row: the cards are narrower, so stack their internals again. */
@media (min-width: 1000px) {
  .teams-grid {
    grid-template-columns: repeat(3, 1fr);
  }

  .team-card {
    flex-direction: column;
    align-items: stretch;
  }

  .team-crest-col {
    width: auto;
    margin: -1.5rem -1.5rem 0;
    padding: 1.5rem 1.5rem 0;
  }

  .team-fields {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
