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
        <!-- Club sheet: crest/name plus every field as a label - value row. -->
        <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-6">
          <div class="team-sheet-head">
            <div class="team-sheet-crest">
              <TeamCrest :team="team" :size="96" />
            </div>
            <h1 class="text-h5 font-weight-bold team-sheet-name">
              {{ team.name }}
              <span v-if="team.shortName" class="text-medium-emphasis"> · {{ team.shortName }}</span>
            </h1>
            <v-btn
              color="red"
              variant="outlined"
              size="large"
              :prepend-icon="mdiSwordCross"
              class="team-sheet-challenge"
              @click="challengeTeam"
            >
              {{ t('profile.team.challenge') }}
            </v-btn>
          </div>

          <dl class="team-sheet-dl team-sheet-dl--main">
            <dt><v-icon size="18" :icon="mdiTagOutline" />{{ t('profile.team.category') }}</dt>
            <dd :style="{ color: AGE_CATEGORY_COLOR[team.category] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.category}`) }}
            </dd>

            <dt><v-icon size="18" :icon="mdiSoccer" />{{ t('profile.team.type') }}</dt>
            <dd :style="{ color: MODALITY_COLOR[team.type] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.type}`) }}
            </dd>

            <dt><v-icon size="18" :icon="mdiTrophyOutline" />{{ t('profile.team.division') }}</dt>
            <dd :style="{ color: DIVISION_COLOR[team.division] }" class="font-weight-bold">
              {{ t(`profile.team.enums.${team.division}`) }}
            </dd>

            <dt><v-icon size="18" :icon="mdiMapMarkerOutline" />{{ t('profile.team.city') }}</dt>
            <dd>{{ team.city }}</dd>

            <dt><v-icon size="18" :icon="mdiAccountOutline" />{{ t('profile.team.coach') }}</dt>
            <dd>{{ team.coach }}</dd>

            <template v-if="team.foundedYear">
              <dt><v-icon size="18" :icon="mdiCalendarOutline" />{{ t('profile.team.foundedYear') }}</dt>
              <dd>{{ team.foundedYear }}</dd>
            </template>
          </dl>

          <template v-if="hasContact">
            <v-divider class="mx-n6 mx-md-n8 mt-4" />
            <div class="team-sheet-contact pt-4">
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
          </template>
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
              size="large"
              :prepend-icon="mdiMapMarkerOutline"
              class="team-sheet-challenge"
            >
              {{ t('profile.team.directions') }}
            </v-btn>
          </div>

          <dl v-if="hasVenue" class="team-sheet-dl team-sheet-dl--main">
            <template v-if="team.venueName">
              <dt><v-icon size="18" :icon="mdiStadiumVariant" />{{ t('profile.team.venueName') }}</dt>
              <dd>{{ team.venueName }}</dd>
            </template>
            <template v-if="team.venueAddress">
              <dt><v-icon size="18" :icon="mdiMapMarkerOutline" />{{ t('profile.team.venueAddress') }}</dt>
              <dd>{{ team.venueAddress }}</dd>
            </template>
            <template v-if="team.venueSurface">
              <dt><v-icon size="18" :icon="mdiGrass" />{{ t('profile.team.venueSurface') }}</dt>
              <dd>{{ t(`profile.team.surfaces.${team.venueSurface}`) }}</dd>
            </template>
          </dl>
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

.team-sheet-name {
  flex: 1 1 auto;
  min-width: 0;
}

.team-sheet-challenge {
  flex-shrink: 0;
  min-width: 12.5rem;
  margin-inline-start: auto;
}

/* Not enough room for crest, name and button on one line - stack the
   button below instead of squeezing the club name. */
@media (max-width: 599px) {
  .team-sheet-challenge {
    width: 100%;
    margin-inline-start: 0;
  }
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
  gap: 0.4rem;
}

/* The primary label/value lists (club sheet, venue) read a little
   larger than any secondary dl on the page. */
.team-sheet-dl--main dt {
  font-size: 0.95rem;
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

.team-sheet-contact {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1.5rem;
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
</style>
