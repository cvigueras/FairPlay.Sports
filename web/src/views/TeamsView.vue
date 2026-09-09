<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import TeamCrest from '@/components/TeamCrest.vue'
import type { Team } from '@/types/team'
import type { PagedResult } from '@/types/pagination'

const { t, locale } = useI18n()
const auth = useAuthStore()

const PAGE_SIZE = 10

const page = ref(1)
const result = ref<PagedResult<Team> | null>(null)
const loading = ref(false)
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    result.value = await teamsApi.page(
      { page: page.value, pageSize: PAGE_SIZE, sort: 'name' },
      auth.accessToken,
    )
  } catch (err) {
    error.value = err instanceof ApiError ? err.message : t('teams.loadFailed')
  } finally {
    loading.value = false
  }
}

watch(page, load, { immediate: true })

const formatLongDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'long', day: 'numeric' })

/** Six fields laid out as two rows of three. */
function fields(team: Team) {
  return [
    { label: t('teams.fields.name'), value: team.name },
    { label: t('teams.fields.coach'), value: team.coach },
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
      <header class="teams-header">
        <h1 class="text-h4 font-weight-bold mb-1">{{ t('teams.title') }}</h1>
        <p v-if="result" class="text-body-2 text-medium-emphasis">
          {{ t('teams.count', { n: result.totalCount }) }}
        </p>
      </header>

      <div class="teams-list">
        <v-progress-circular
          v-if="loading"
          indeterminate
          color="primary"
          class="d-block mx-auto my-16"
        />
        <v-alert v-else-if="error" type="error" variant="tonal">{{ error }}</v-alert>

        <template v-else-if="result">
          <p v-if="result.items.length === 0" class="text-body-1 text-medium-emphasis">
            {{ t('teams.empty') }}
          </p>

          <v-card
            v-for="team in result.items"
            :key="team.id"
            border
            flat
            rounded="xl"
            class="team-card mb-4 pa-4 pa-sm-6"
          >
            <div class="team-crest-col">
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

            <div class="team-fields">
              <div v-for="field in fields(team)" :key="field.label" class="team-field">
                <div class="text-caption text-medium-emphasis">{{ field.label }}</div>
                <v-divider class="my-1" />
                <div class="text-body-1 font-weight-medium">{{ field.value }}</div>
              </div>
            </div>
          </v-card>
        </template>
      </div>

      <footer v-if="result && result.totalPages > 1" class="teams-footer">
        <v-pagination
          v-model="page"
          :length="result.totalPages"
          :total-visible="7"
          rounded="circle"
          density="comfortable"
        />
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

.teams-header {
  flex: 0 0 auto;
  padding-bottom: 1rem;
}

.teams-list {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding-bottom: 1rem;
}

.teams-footer {
  flex: 0 0 auto;
  display: flex;
  justify-content: center;
  padding: 0.75rem 0 1rem;
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-background));
}

.team-card {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.team-crest-col {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 0.85rem;
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
  }
}

@media (min-width: 860px) {
  .team-fields {
    grid-template-columns: repeat(3, 1fr);
  }
}
</style>
