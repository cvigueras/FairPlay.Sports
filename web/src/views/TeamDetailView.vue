<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiArrowLeft,
  mdiEmailOutline,
  mdiMapMarkerOutline,
  mdiPaletteOutline,
  mdiPhoneOutline,
  mdiSoccerField,
  mdiSwordCross,
  mdiWeb,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import type { Team } from '@/types/team'

const props = defineProps<{ id: string }>()

const { t } = useI18n()
const auth = useAuthStore()
const ui = useUiStore()

const team = ref<Team | null>(null)
const loading = ref(false)
const notFound = ref(false)
const error = ref('')

async function load(id: string) {
  loading.value = true
  notFound.value = false
  error.value = ''
  team.value = null
  try {
    team.value = await teamsApi.byId(id, auth.accessToken)
    ui.breadcrumbLabel = team.value.name
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) {
      notFound.value = true
    } else {
      error.value = err instanceof ApiError ? err.message : t('teams.detail.loadFailed')
    }
  } finally {
    loading.value = false
  }
}

watch(() => props.id, load, { immediate: true })

onUnmounted(() => {
  ui.breadcrumbLabel = null
})

/** A Google Maps link: the club's own link if given, otherwise a search by address/name + city. */
const mapsHref = computed(() => {
  if (!team.value) return null
  if (team.value.venueMapsUrl) return team.value.venueMapsUrl
  const query = [team.value.venueAddress || team.value.venueName, team.value.city]
    .filter(Boolean)
    .join(', ')
  return query ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}` : null
})

const hasVenue = computed(
  () => !!team.value && (team.value.venueName || team.value.venueAddress || team.value.venueSurface),
)

const hasKit = computed(() => !!team.value && !!(team.value.colorPrimary || team.value.colorSecondary))

const HEX_COLOR = /^#([0-9a-f]{3}|[0-9a-f]{6})$/i
const isHexColor = (value: string) => HEX_COLOR.test(value.trim())

function challengeTeam() {
  // TODO: wire up the team-vs-team challenge flow.
}
</script>

<template>
  <v-main>
    <v-container class="py-6 py-md-10 team-detail-container">
      <v-btn
        :to="{ name: 'teams' }"
        :prepend-icon="mdiArrowLeft"
        variant="text"
        size="small"
        class="mb-4"
      >
        {{ t('nav.teams') }}
      </v-btn>

      <v-progress-circular
        v-if="loading"
        indeterminate
        color="primary"
        class="d-block mx-auto my-16"
      />

      <v-alert v-else-if="notFound" type="warning" variant="tonal">
        {{ t('teams.detail.notFound') }}
      </v-alert>

      <v-alert v-else-if="error" type="error" variant="tonal">
        {{ error }}
      </v-alert>

      <template v-else-if="team">
        <!-- Club sheet: crest/name plus every field as a label - value row. -->
        <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-6">
          <div class="team-sheet-head">
            <div class="team-sheet-crest">
              <TeamCrest :team="team" :size="96" />
            </div>
            <h1 class="text-h5 font-weight-bold">
              {{ team.name }}
              <span v-if="team.shortName" class="text-medium-emphasis"> · {{ team.shortName }}</span>
            </h1>
          </div>

          <dl class="team-sheet-dl">
            <dt>{{ t('profile.team.category') }}</dt>
            <dd :style="{ color: AGE_CATEGORY_COLOR[team.category] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.category}`) }}
            </dd>

            <dt>{{ t('profile.team.type') }}</dt>
            <dd :style="{ color: MODALITY_COLOR[team.type] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.type}`) }}
            </dd>

            <dt>{{ t('profile.team.division') }}</dt>
            <dd :style="{ color: DIVISION_COLOR[team.division] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.division}`) }}
            </dd>

            <dt>{{ t('profile.team.city') }}</dt>
            <dd>{{ team.city }}</dd>

            <dt>{{ t('profile.team.coach') }}</dt>
            <dd>{{ team.coach }}</dd>

            <template v-if="team.foundedYear">
              <dt>{{ t('profile.team.foundedYear') }}</dt>
              <dd>{{ team.foundedYear }}</dd>
            </template>

            <template v-if="team.contactEmail">
              <dt><v-icon size="16" :icon="mdiEmailOutline" /> {{ t('profile.team.contactEmail') }}</dt>
              <dd><a :href="`mailto:${team.contactEmail}`">{{ team.contactEmail }}</a></dd>
            </template>
            <template v-if="team.contactPhone">
              <dt><v-icon size="16" :icon="mdiPhoneOutline" /> {{ t('profile.team.contactPhone') }}</dt>
              <dd><a :href="`tel:${team.contactPhone}`">{{ team.contactPhone }}</a></dd>
            </template>
            <template v-if="team.website">
              <dt><v-icon size="16" :icon="mdiWeb" /> {{ t('profile.team.website') }}</dt>
              <dd>
                <a :href="team.website" target="_blank" rel="noopener">{{ team.website }}</a>
              </dd>
            </template>
          </dl>
        </v-card>

        <!-- Equipación (kit colours) -->
        <v-card border flat rounded="xl" class="pa-6 mb-6">
          <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
            <v-icon :icon="mdiPaletteOutline" />
            {{ t('profile.team.colorsGroup') }}
          </h2>

          <div v-if="hasKit" class="team-sheet-kit">
            <span v-if="team.colorPrimary" class="team-sheet-kit-item">
              <i
                v-if="isHexColor(team.colorPrimary)"
                class="team-sheet-swatch"
                :style="{ background: team.colorPrimary }"
              />
              <span>
                <span class="team-sheet-kit-label">{{ t('profile.team.colorPrimary') }}</span>
                <span class="font-weight-bold d-block">{{ team.colorPrimary }}</span>
              </span>
            </span>
            <span v-if="team.colorSecondary" class="team-sheet-kit-item">
              <i
                v-if="isHexColor(team.colorSecondary)"
                class="team-sheet-swatch"
                :style="{ background: team.colorSecondary }"
              />
              <span>
                <span class="team-sheet-kit-label">{{ t('profile.team.colorSecondary') }}</span>
                <span class="font-weight-bold d-block">{{ team.colorSecondary }}</span>
              </span>
            </span>
          </div>
          <p v-else class="text-body-2 text-medium-emphasis">
            {{ t('teams.detail.noProfile') }}
          </p>
        </v-card>

        <!-- Campo (home venue) -->
        <v-card border flat rounded="xl" class="pa-6 mb-6">
          <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
            <v-icon :icon="mdiSoccerField" color="#2E7D32" />
            {{ t('profile.team.venueGroup') }}
          </h2>

          <template v-if="hasVenue">
            <dl class="team-sheet-dl">
              <template v-if="team.venueName">
                <dt>{{ t('profile.team.venueName') }}</dt>
                <dd>{{ team.venueName }}</dd>
              </template>
              <template v-if="team.venueAddress">
                <dt>{{ t('profile.team.venueAddress') }}</dt>
                <dd>{{ team.venueAddress }}</dd>
              </template>
              <template v-if="team.venueSurface">
                <dt>{{ t('profile.team.venueSurface') }}</dt>
                <dd>{{ t(`profile.team.surfaces.${team.venueSurface}`) }}</dd>
              </template>
            </dl>

            <v-btn
              v-if="mapsHref"
              :href="mapsHref"
              target="_blank"
              rel="noopener"
              variant="tonal"
              size="small"
              :prepend-icon="mdiMapMarkerOutline"
              class="mt-4"
            >
              {{ t('profile.team.directions') }}
            </v-btn>
          </template>
          <p v-else class="text-body-2 text-medium-emphasis">
            {{ t('teams.detail.noVenue') }}
          </p>
        </v-card>

        <div class="team-detail-challenge">
          <v-btn
            color="red"
            variant="flat"
            size="x-large"
            :prepend-icon="mdiSwordCross"
            @click="challengeTeam"
          >
            {{ t('profile.team.challenge') }}
          </v-btn>
        </div>
      </template>
    </v-container>
  </v-main>
</template>

<style scoped>
.team-detail-container {
  max-width: 800px;
}

.team-sheet-head {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.team-sheet-crest {
  flex-shrink: 0;
}

.team-sheet-dl {
  display: grid;
  grid-template-columns: auto 1fr;
  column-gap: 1.5rem;
  row-gap: 0.75rem;
  margin: 0;
  align-items: center;
}

.team-sheet-dl dt {
  font-size: 0.8rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.team-sheet-dl dd {
  margin: 0;
}

.team-sheet-dl dd a {
  color: rgb(var(--v-theme-primary));
  text-decoration: none;
}

.team-sheet-dl dd a:hover {
  text-decoration: underline;
}

.team-sheet-kit {
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
}

.team-sheet-kit-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.team-sheet-kit-label {
  display: block;
  font-size: 0.75rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-sheet-swatch {
  display: inline-block;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.team-detail-challenge {
  display: flex;
  justify-content: center;
  margin-top: 1rem;
}
</style>
