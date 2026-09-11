<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiAccountOutline,
  mdiArrowLeft,
  mdiCalendarOutline,
  mdiEmailOutline,
  mdiGrass,
  mdiMapMarkerOutline,
  mdiPaletteOutline,
  mdiPhoneOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiStadiumVariant,
  mdiSwordCross,
  mdiTagOutline,
  mdiTrophyOutline,
  mdiTshirtCrew,
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
import { SURFACE_COLOR } from '@/lib/pitchSurface'
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
        <!-- Hero: crest, identity, category/type/division badges and the key facts. -->
        <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5">
          <div class="team-hero-head">
            <div class="team-hero-crest">
              <TeamCrest :team="team" :size="112" />
            </div>
            <div class="team-hero-info">
              <h1 class="text-h4 font-weight-bold team-hero-name">
                {{ team.name }}
                <span v-if="team.shortName" class="text-medium-emphasis text-h6"> · {{ team.shortName }}</span>
              </h1>

              <div class="team-hero-chips">
                <v-chip size="small" variant="tonal" :color="AGE_CATEGORY_COLOR[team.category]" :prepend-icon="mdiTagOutline">
                  {{ t(`profile.team.enums.${team.category}`) }}
                </v-chip>
                <v-chip size="small" variant="tonal" :color="MODALITY_COLOR[team.type]" :prepend-icon="mdiSoccer">
                  {{ t(`profile.team.enums.${team.type}`) }}
                </v-chip>
                <v-chip size="small" variant="tonal" :color="DIVISION_COLOR[team.division]" :prepend-icon="mdiTrophyOutline">
                  {{ t(`profile.team.enums.${team.division}`) }}
                </v-chip>
              </div>

              <div class="team-hero-facts text-body-2 text-medium-emphasis">
                <span><v-icon size="16" :icon="mdiMapMarkerOutline" />{{ team.city }}</span>
                <span><v-icon size="16" :icon="mdiAccountOutline" color="#5D4037" />{{ team.coach }}</span>
                <span v-if="team.foundedYear">
                  <v-icon size="16" :icon="mdiCalendarOutline" />{{ t('profile.team.foundedYear') }} {{ team.foundedYear }}
                </span>
              </div>
            </div>

            <v-btn
              color="red"
              variant="outlined"
              size="large"
              :prepend-icon="mdiSwordCross"
              class="team-hero-challenge"
              @click="challengeTeam"
            >
              {{ t('profile.team.challenge') }}
            </v-btn>
          </div>
        </v-card>

        <!-- Contacto and Equipación: a matched-height row on desktop. -->
        <div class="team-row-2col mb-5" :class="{ 'team-row-2col--single': !hasContact }">
          <v-card v-if="hasContact" border flat rounded="xl" class="pa-6 pa-md-8 team-panel">
            <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
              <v-icon :icon="mdiEmailOutline" />
              {{ t('profile.team.contactGroup') }}
            </h2>
            <div class="team-sheet-contact team-sheet-contact--stacked">
              <a v-if="team.contactEmail" :href="`mailto:${team.contactEmail}`" class="team-sheet-contact-item">
                <v-icon size="18" :icon="mdiEmailOutline" />
                <span>{{ team.contactEmail }}</span>
              </a>
              <a v-if="team.contactPhone" :href="`tel:${team.contactPhone}`" class="team-sheet-contact-item">
                <v-icon size="18" :icon="mdiPhoneOutline" />
                <span>{{ team.contactPhone }}</span>
              </a>
              <a
                v-if="team.website"
                :href="team.website"
                target="_blank"
                rel="noopener"
                class="team-sheet-contact-item"
              >
                <v-icon size="18" :icon="mdiWeb" />
                <span>{{ team.website }}</span>
              </a>
            </div>
          </v-card>

          <v-card border flat rounded="xl" class="pa-6 pa-md-8 team-panel team-panel--center">
            <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
              <v-icon :icon="mdiPaletteOutline" />
              {{ t('profile.team.colorsGroup') }}
            </h2>

            <div v-if="hasKit" class="team-sheet-kit">
              <span v-if="team.colorPrimary" class="team-sheet-kit-item">
                <v-icon v-if="isHexColor(team.colorPrimary)" :icon="mdiTshirtCrew" :color="team.colorPrimary" size="48" />
                <span class="team-sheet-kit-label">{{ t('profile.team.colorPrimary') }}</span>
              </span>
              <span v-if="team.colorSecondary" class="team-sheet-kit-item">
                <v-icon v-if="isHexColor(team.colorSecondary)" :icon="mdiTshirtCrew" :color="team.colorSecondary" size="48" />
                <span class="team-sheet-kit-label">{{ t('profile.team.colorSecondary') }}</span>
              </span>
            </div>
            <p v-else class="text-body-2 text-medium-emphasis">
              {{ t('teams.detail.noProfile') }}
            </p>
          </v-card>
        </div>

        <!-- Campo (home venue): full width, fields laid out in a row. -->
        <v-card border flat rounded="xl" class="pa-6 pa-md-8">
          <div class="team-sheet-head mb-4">
            <h2 class="text-subtitle-1 font-weight-bold d-flex align-center ga-2 team-sheet-name">
              <v-icon :icon="mdiSoccerField" color="#2E7D32" />
              {{ t('profile.team.venueGroup') }}
            </h2>
            <v-btn
              v-if="hasVenue && mapsHref"
              :href="mapsHref"
              target="_blank"
              rel="noopener"
              color="#2E7D32"
              variant="outlined"
              :prepend-icon="mdiMapMarkerOutline"
              class="team-venue-directions"
            >
              {{ t('profile.team.directions') }}
            </v-btn>
          </div>

          <div v-if="hasVenue" class="team-venue-facts">
            <span v-if="team.venueName" class="text-body-2">
              <v-icon size="18" :icon="mdiStadiumVariant" color="#64748b" />
              {{ team.venueName }}
            </span>
            <span v-if="team.venueAddress" class="text-body-2">
              <v-icon size="18" :icon="mdiMapMarkerOutline" color="#64748b" />
              {{ team.venueAddress }}
            </span>
            <v-chip
              v-if="team.venueSurface"
              size="small"
              variant="tonal"
              :color="SURFACE_COLOR[team.venueSurface]"
              :prepend-icon="mdiGrass"
            >
              {{ t(`profile.team.surfaces.${team.venueSurface}`) }}
            </v-chip>
          </div>
          <p v-else class="text-body-2 text-medium-emphasis">
            {{ t('teams.detail.noVenue') }}
          </p>
        </v-card>
      </template>
    </v-container>
  </v-main>
</template>

<style scoped>
.team-detail-container {
  max-width: 960px;
}

/* Hero: crest, name/badges/facts, and the challenge action. */
.team-hero-head {
  display: flex;
  align-items: flex-start;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.team-hero-crest {
  flex-shrink: 0;
}

.team-hero-info {
  flex: 1 1 260px;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.team-hero-name {
  margin: 0;
}

.team-hero-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.team-hero-facts {
  display: flex;
  flex-wrap: wrap;
  gap: 1.25rem;
}

.team-hero-facts span {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.team-hero-challenge {
  flex-shrink: 0;
  min-width: 12.5rem;
  margin-inline-start: auto;
}

/* Not enough room for crest, name and button on one line - stack the
   button below instead of squeezing the club name. */
@media (max-width: 599px) {
  .team-hero-challenge {
    width: 100%;
    margin-inline-start: 0;
  }
}

.team-sheet-contact {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1.5rem;
}

.team-sheet-contact--stacked {
  flex-direction: column;
  flex-wrap: nowrap;
  gap: 0.85rem;
}

.team-sheet-contact-item {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  color: inherit;
  text-decoration: none;
}

.team-sheet-contact-item .v-icon {
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-sheet-contact-item:hover {
  text-decoration: underline;
}

/* Contacto and Equipación: a matched-height row on desktop, stacked on phones. */
.team-row-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  align-items: stretch;
}

.team-row-2col--single {
  grid-template-columns: 1fr;
}

@media (max-width: 899px) {
  .team-row-2col {
    grid-template-columns: 1fr;
  }
}

.team-panel {
  display: flex;
  flex-direction: column;
}

/* Equipación has less content than Contacto - centre it in the matched height
   instead of leaving it pinned to the top with dead space below. */
.team-panel--center {
  justify-content: center;
}

.team-sheet-kit {
  flex: 1;
  display: flex;
  align-items: center;
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
  font-size: 1rem;
  font-weight: 600;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-sheet-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.team-sheet-name {
  min-width: 0;
}

.team-venue-directions {
  flex-shrink: 0;
}

.team-venue-facts {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 1.75rem;
}

.team-venue-facts span {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
