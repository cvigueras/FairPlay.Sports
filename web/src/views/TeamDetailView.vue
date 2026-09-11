<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiArrowLeft,
  mdiCalendarOutline,
  mdiEmailOutline,
  mdiMapMarkerOutline,
  mdiPaletteOutline,
  mdiPhoneOutline,
  mdiShieldOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiSwordCross,
  mdiTrophyOutline,
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

const hasColors = computed(
  () => !!team.value && !!(team.value.colorPrimary || team.value.colorSecondary),
)

const hasContact = computed(
  () => !!team.value && !!(team.value.contactEmail || team.value.contactPhone || team.value.website),
)

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
        <!-- Hero: crest, name, category/type/division/city, coach -->
        <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-6 team-hero">
          <div class="team-hero-crest">
            <TeamCrest :team="team" :size="112" />
          </div>
          <div class="team-hero-info">
            <h1 class="text-h5 font-weight-bold">
              {{ team.name }}
              <span v-if="team.shortName" class="text-medium-emphasis"> · {{ team.shortName }}</span>
            </h1>
            <div class="team-hero-chips">
              <v-chip
                size="small"
                variant="tonal"
                :style="{ color: AGE_CATEGORY_COLOR[team.category] }"
              >
                {{ t(`profile.team.enums.${team.category}`) }}
              </v-chip>
              <v-chip
                size="small"
                variant="tonal"
                :color="MODALITY_COLOR[team.type]"
                :prepend-icon="mdiSoccer"
              >
                {{ t(`profile.team.enums.${team.type}`) }}
              </v-chip>
              <v-chip
                size="small"
                variant="tonal"
                :color="DIVISION_COLOR[team.division]"
                :prepend-icon="mdiTrophyOutline"
              >
                {{ t(`profile.team.enums.${team.division}`) }}
              </v-chip>
              <v-chip size="small" variant="tonal" :prepend-icon="mdiMapMarkerOutline">
                {{ team.city }}
              </v-chip>
            </div>
            <p class="text-body-2 text-medium-emphasis mt-2 team-hero-meta">
              <span>{{ team.coach }}</span>
              <span v-if="team.foundedYear">
                <v-icon size="14" :icon="mdiCalendarOutline" />
                {{ t('profile.team.foundedYear') }}: {{ team.foundedYear }}
              </span>
            </p>
          </div>
        </v-card>

        <v-row>
          <!-- Venue -->
          <v-col cols="12" md="6">
            <v-card border flat rounded="xl" class="pa-6 h-100">
              <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
                <v-icon :icon="mdiSoccerField" color="#2E7D32" />
                {{ t('profile.team.venueGroup') }}
              </h2>

              <template v-if="hasVenue">
                <dl class="team-detail-dl">
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
          </v-col>

          <!-- Colors + contact -->
          <v-col cols="12" md="6">
            <v-card border flat rounded="xl" class="pa-6 h-100">
              <template v-if="hasColors">
                <h2 class="text-subtitle-1 font-weight-bold mb-3 d-flex align-center ga-2">
                  <v-icon :icon="mdiPaletteOutline" />
                  {{ t('profile.team.colorsGroup') }}
                </h2>
                <div class="team-detail-colors mb-4">
                  <span v-if="team.colorPrimary" class="team-detail-color">
                    <i
                      v-if="isHexColor(team.colorPrimary)"
                      class="team-detail-swatch"
                      :style="{ background: team.colorPrimary }"
                    />
                    {{ team.colorPrimary }}
                  </span>
                  <span v-if="team.colorSecondary" class="team-detail-color">
                    <i
                      v-if="isHexColor(team.colorSecondary)"
                      class="team-detail-swatch"
                      :style="{ background: team.colorSecondary }"
                    />
                    {{ team.colorSecondary }}
                  </span>
                </div>
              </template>

              <template v-if="hasContact">
                <h2 class="text-subtitle-1 font-weight-bold mb-3 d-flex align-center ga-2">
                  <v-icon :icon="mdiEmailOutline" />
                  {{ t('profile.team.contactGroup') }}
                </h2>
                <dl class="team-detail-dl">
                  <template v-if="team.contactEmail">
                    <dt><v-icon size="16" :icon="mdiEmailOutline" /></dt>
                    <dd><a :href="`mailto:${team.contactEmail}`">{{ team.contactEmail }}</a></dd>
                  </template>
                  <template v-if="team.contactPhone">
                    <dt><v-icon size="16" :icon="mdiPhoneOutline" /></dt>
                    <dd><a :href="`tel:${team.contactPhone}`">{{ team.contactPhone }}</a></dd>
                  </template>
                  <template v-if="team.website">
                    <dt><v-icon size="16" :icon="mdiWeb" /></dt>
                    <dd>
                      <a :href="team.website" target="_blank" rel="noopener">{{ team.website }}</a>
                    </dd>
                  </template>
                </dl>
              </template>

              <p v-if="!hasColors && !hasContact" class="text-body-2 text-medium-emphasis">
                <v-icon :icon="mdiShieldOutline" size="18" class="me-1" />
                {{ t('teams.detail.noProfile') }}
              </p>
            </v-card>
          </v-col>
        </v-row>

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
  max-width: 1100px;
}

.team-hero {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.team-hero-crest {
  flex-shrink: 0;
}

.team-hero-info {
  flex: 1;
  min-width: 0;
}

.team-hero-chips {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
  margin-top: 0.5rem;
}

.team-hero-meta {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  flex-wrap: wrap;
}

.team-hero-meta span {
  display: flex;
  align-items: center;
  gap: 0.3rem;
}

.team-detail-dl {
  display: grid;
  grid-template-columns: auto 1fr;
  column-gap: 1rem;
  row-gap: 0.5rem;
  margin: 0;
  align-items: center;
}

.team-detail-dl dt {
  font-size: 0.75rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
  display: flex;
  align-items: center;
}

.team-detail-dl dd {
  margin: 0;
  font-weight: 500;
}

.team-detail-dl dd a {
  color: rgb(var(--v-theme-primary));
  text-decoration: none;
}

.team-detail-dl dd a:hover {
  text-decoration: underline;
}

.team-detail-colors {
  display: flex;
  gap: 1.25rem;
  flex-wrap: wrap;
}

.team-detail-color {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-weight: 500;
}

.team-detail-swatch {
  display: inline-block;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.team-detail-challenge {
  display: flex;
  justify-content: center;
  margin-top: 2rem;
}
</style>
