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
import { CHALLENGE_ACTOR_ROLES, challengesApi } from '@/lib/challenges'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import ChallengeWizard from '@/components/ChallengeWizard.vue'
import KitPreview from '@/components/KitPreview.vue'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { SURFACE_COLOR } from '@/lib/pitchSurface'
import type { SendChallengePayload } from '@/types/challenge'
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
    // Best-effort: only used to decide whether to show the challenge button.
    auth.loadMyTeams().catch(() => {})
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

const firstKit = computed(() => {
  const t = team.value
  if (!t?.colorPrimary || !t.colorSecondary || !t.kitPattern || !t.shortsColor) return null
  return { pattern: t.kitPattern, primary: t.colorPrimary, secondary: t.colorSecondary, shorts: t.shortsColor }
})

const secondKit = computed(() => {
  const t = team.value
  if (!t?.alternateColorPrimary || !t.alternateColorSecondary || !t.alternateKitPattern || !t.alternateShortsColor) {
    return null
  }
  return {
    pattern: t.alternateKitPattern,
    primary: t.alternateColorPrimary,
    secondary: t.alternateColorSecondary,
    shorts: t.alternateShortsColor,
  }
})

const hasKit = computed(() => !!firstKit.value || !!secondKit.value)

const hasContact = computed(
  () => !!team.value && !!(team.value.contactEmail || team.value.contactPhone || team.value.website),
)

/** Never your own team (any role), and only for users who can actually act for
 *  some team (a pure Player, with no Delegate/Coach/President/TechnicalStaff
 *  membership anywhere, has no team to send a challenge from). */
const canChallenge = computed(() => {
  if (!team.value) return false
  const teamId = team.value.id
  if (auth.myTeams.some((membership) => membership.teamId === teamId)) return false
  return auth.myTeams.some((membership) => CHALLENGE_ACTOR_ROLES.includes(membership.role))
})

const challengeWizardOpen = ref(false)
const sendingChallenge = ref(false)

function challengeTeam() {
  challengeWizardOpen.value = true
}

async function handleSendChallenge(payload: SendChallengePayload) {
  sendingChallenge.value = true
  try {
    await challengesApi.send(payload, auth.accessToken)
    challengeWizardOpen.value = false
    ui.notify(t('challenges.wizard.sentSuccess', { team: team.value?.name }))
  } catch (err) {
    ui.notify(err instanceof ApiError ? err.message : t('challenges.wizard.sendFailed'), 'error')
  } finally {
    sendingChallenge.value = false
  }
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
              v-if="canChallenge"
              color="red"
              variant="flat"
              size="large"
              :prepend-icon="mdiSwordCross"
              class="team-hero-challenge"
              @click="challengeTeam"
            >
              {{ t('profile.team.challenge') }}
            </v-btn>
          </div>
        </v-card>

        <!-- Contacto and Equipación: a matched-height row on desktop. Contacto always
             renders (an empty state when there's nothing) so the row never reflows
             to a single column depending on which fields a team happened to fill in. -->
        <div class="team-row-2col mb-5">
          <v-card border flat rounded="xl" class="pa-6 pa-md-8 team-panel">
            <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
              <v-icon :icon="mdiEmailOutline" />
              {{ t('profile.team.contactGroup') }}
            </h2>

            <div v-if="hasContact" class="team-contact-list">
              <a v-if="team.contactEmail" :href="`mailto:${team.contactEmail}`" class="team-contact-row">
                <span class="team-contact-icon"><v-icon size="18" :icon="mdiEmailOutline" /></span>
                <span class="team-contact-text">
                  <span class="team-contact-label">{{ t('profile.team.contactEmail') }}</span>
                  <span class="team-contact-value">{{ team.contactEmail }}</span>
                </span>
              </a>
              <a v-if="team.contactPhone" :href="`tel:${team.contactPhone}`" class="team-contact-row">
                <span class="team-contact-icon"><v-icon size="18" :icon="mdiPhoneOutline" /></span>
                <span class="team-contact-text">
                  <span class="team-contact-label">{{ t('profile.team.contactPhone') }}</span>
                  <span class="team-contact-value">{{ team.contactPhone }}</span>
                </span>
              </a>
              <a
                v-if="team.website"
                :href="team.website"
                target="_blank"
                rel="noopener"
                class="team-contact-row"
              >
                <span class="team-contact-icon"><v-icon size="18" :icon="mdiWeb" /></span>
                <span class="team-contact-text">
                  <span class="team-contact-label">{{ t('profile.team.website') }}</span>
                  <span class="team-contact-value">{{ team.website }}</span>
                </span>
              </a>
            </div>
            <div v-else class="team-empty-state">
              <v-icon size="28" :icon="mdiEmailOutline" />
              <p class="text-body-2 mb-0">{{ t('teams.detail.noContact') }}</p>
            </div>
          </v-card>

          <v-card border flat rounded="xl" class="pa-6 pa-md-8 team-panel team-panel--center">
            <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
              <span class="team-panel-icon">
                <svg width="16" height="16" viewBox="0 0 100 100" aria-hidden="true">
                  <path
                    d="M6,24 C2,28 2,34 5,38 L24,52 L24,86 C24,91 28,95 33,95 L67,95 C72,95 76,91 76,86 L76,52 L95,38 C98,34 98,28 94,24 L75,9 C73,8 70,9 69,11 C65,19 58,23 50,23 C42,23 35,19 31,11 C30,9 27,8 25,9 Z"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="6"
                    stroke-linejoin="round"
                  />
                </svg>
              </span>
              {{ t('profile.team.colorsGroup') }}
            </h2>

            <div v-if="hasKit" class="team-sheet-kit">
              <div
                v-if="firstKit"
                class="team-kit-stage"
                :style="{
                  background: `color-mix(in srgb, ${firstKit.primary} 7%, white)`,
                  borderColor: `color-mix(in srgb, ${firstKit.primary} 22%, white)`,
                }"
              >
                <span class="team-kit-tag">{{ t('profile.team.wizard.stepKit') }}</span>
                <KitPreview
                  :pattern="firstKit.pattern"
                  :primary="firstKit.primary"
                  :secondary="firstKit.secondary"
                  :shorts="firstKit.shorts"
                  :size="120"
                />
                <div class="team-kit-legend">
                  <span class="team-kit-swatch" :style="{ background: firstKit.primary }" />
                  <span class="team-kit-swatch" :style="{ background: firstKit.secondary }" />
                  <span class="team-kit-swatch" :style="{ background: firstKit.shorts }" />
                </div>
              </div>
              <div
                v-if="secondKit"
                class="team-kit-stage"
                :style="{
                  background: `color-mix(in srgb, ${secondKit.primary} 7%, white)`,
                  borderColor: `color-mix(in srgb, ${secondKit.primary} 22%, white)`,
                }"
              >
                <span class="team-kit-tag">{{ t('profile.team.wizard.stepKitSecondary') }}</span>
                <KitPreview
                  :pattern="secondKit.pattern"
                  :primary="secondKit.primary"
                  :secondary="secondKit.secondary"
                  :shorts="secondKit.shorts"
                  :size="120"
                />
                <div class="team-kit-legend">
                  <span class="team-kit-swatch" :style="{ background: secondKit.primary }" />
                  <span class="team-kit-swatch" :style="{ background: secondKit.secondary }" />
                  <span class="team-kit-swatch" :style="{ background: secondKit.shorts }" />
                </div>
              </div>
            </div>
            <div v-else class="team-empty-state">
              <v-icon size="28" :icon="mdiPaletteOutline" />
              <p class="text-body-2 mb-0">{{ t('teams.detail.noProfile') }}</p>
            </div>
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
          <div v-else class="team-empty-state">
            <v-icon size="28" :icon="mdiSoccerField" />
            <p class="text-body-2 mb-0">{{ t('teams.detail.noVenue') }}</p>
          </div>
        </v-card>

        <ChallengeWizard
          v-if="canChallenge"
          v-model="challengeWizardOpen"
          :rival-team="team"
          :loading="sendingChallenge"
          @submit="handleSendChallenge"
        />
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

.team-contact-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.team-contact-row {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  color: inherit;
  text-decoration: none;
}

.team-contact-icon {
  width: 2.375rem;
  height: 2.375rem;
  border-radius: 0.75rem;
  background: rgba(var(--v-theme-primary), 0.08);
  color: rgb(var(--v-theme-primary));
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.team-contact-text {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  min-width: 0;
}

.team-contact-label {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-contact-value {
  font-size: 0.9375rem;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.team-contact-row:hover .team-contact-value {
  text-decoration: underline;
}

/* A shared fallback for a panel with nothing to show (contact, kit, venue) -
   the panel keeps its place in the layout instead of collapsing or disappearing. */
.team-empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  text-align: center;
  padding: 1.5rem;
  border: 1.5px dashed rgba(var(--v-theme-on-surface), 0.16);
  border-radius: 0.875rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

/* Contacto and Equipación: a matched-height row on desktop, stacked on phones. */
.team-row-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  align-items: stretch;
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
  align-items: stretch;
  gap: 1rem;
  flex-wrap: wrap;
}

.team-kit-stage {
  flex: 1 1 140px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.6rem;
  padding: 1.25rem 1rem;
  border: 1px solid;
  border-radius: 1rem;
}

.team-panel-icon {
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 0.5rem;
  background: rgba(var(--v-theme-primary), 0.1);
  color: rgb(var(--v-theme-primary));
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.team-kit-tag {
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  background: #ffffff;
  color: rgb(var(--v-theme-on-surface));
}

.team-kit-legend {
  display: flex;
  gap: 0.4rem;
}

.team-kit-swatch {
  width: 0.8rem;
  height: 0.8rem;
  border-radius: 0.25rem;
  border: 1px solid rgba(15, 23, 42, 0.15);
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
