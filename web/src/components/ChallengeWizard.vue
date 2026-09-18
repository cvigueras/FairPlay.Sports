<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiCalendarOutline,
  mdiCheck,
  mdiChevronLeft,
  mdiChevronRight,
  mdiClockOutline,
  mdiClose,
  mdiMapMarkerOutline,
} from '@mdi/js'
import FlatField from '@/components/FlatField.vue'
import KitPreview from '@/components/KitPreview.vue'
import TeamCrest from '@/components/TeamCrest.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { CHALLENGE_ACTOR_ROLES } from '@/lib/challenges'
import { DIVISION_COLOR } from '@/lib/division'
import { homeKit, resolveAwayKit, type ResolvedKit } from '@/lib/kitClash'
import { MODALITY_COLOR } from '@/lib/modality'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { SendChallengePayload } from '@/types/challenge'
import type { Team } from '@/types/team'

const props = withDefaults(defineProps<{ modelValue: boolean; rivalTeam: Team; loading?: boolean }>(), {
  loading: false,
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'submit', value: SendChallengePayload): void
}>()

const { t } = useI18n()
const auth = useAuthStore()
const ui = useUiStore()

const TOTAL_STEPS = 3
const step = ref(1)
const STEP_TITLE_KEYS = ['stepTeam', 'stepMatch', 'stepSummary']
const stepTitle = computed(() => t(`challenges.wizard.${STEP_TITLE_KEYS[step.value - 1]}`))

/* ---- Step 1: which of my teams sends the challenge --------------------- */

const eligibleTeams = ref<Team[]>([])
const loadingTeams = ref(false)
const selectedTeamId = ref<string | null>(null)

async function loadEligibleTeams() {
  loadingTeams.value = true
  try {
    await auth.loadMyTeams()
    const memberships = auth.myTeams.filter(
      (m) => CHALLENGE_ACTOR_ROLES.includes(m.role) && m.teamId !== props.rivalTeam.id,
    )
    eligibleTeams.value = await Promise.all(memberships.map((m) => teamsApi.byId(m.teamId, auth.accessToken)))
  } catch {
    ui.notify(t('teams.detail.loadFailed'), 'error')
  } finally {
    loadingTeams.value = false
  }
}

function roleFor(teamId: string): string | undefined {
  return auth.myTeams.find((m) => m.teamId === teamId)?.role
}

const selectedTeam = computed(() => eligibleTeams.value.find((team) => team.id === selectedTeamId.value) ?? null)

/* ---- Step 2: venue (decides home/away), date, time, kit ----------------- */

const venueChoice = ref<'mine' | 'rival'>('mine')
const matchDateStr = ref('')
const matchTimeStr = ref('')
const todayStr = new Date().toISOString().slice(0, 10)

const homeTeam = computed<Team | null>(() => {
  if (!selectedTeam.value) return null
  return venueChoice.value === 'mine' ? selectedTeam.value : props.rivalTeam
})
const awayTeam = computed<Team | null>(() => {
  if (!selectedTeam.value) return null
  return venueChoice.value === 'mine' ? props.rivalTeam : selectedTeam.value
})

function hasVenue(team: Team): boolean {
  return !!(team.venueName || team.venueAddress || team.venueSurface)
}

const homeKitPreview = computed<ResolvedKit | null>(() => (homeTeam.value ? homeKit(homeTeam.value) : null))
const awayKitPreview = computed<ResolvedKit | null>(() =>
  homeTeam.value && awayTeam.value ? resolveAwayKit(homeTeam.value, awayTeam.value) : null,
)
const kitBlocked = computed(() => !!homeTeam.value && (!homeKitPreview.value || !awayKitPreview.value))

const matchDateTime = computed<Date | null>(() => {
  if (!matchDateStr.value || !matchTimeStr.value) return null
  const date = new Date(`${matchDateStr.value}T${matchTimeStr.value}`)
  return Number.isNaN(date.getTime()) ? null : date
})
const isDateValid = computed(() => !!matchDateTime.value && matchDateTime.value.getTime() > Date.now())

/* ---- Step 3: optional message ------------------------------------------- */

const message = ref('')

/* ---- Navigation ----------------------------------------------------------- */

function stepIsValid(n: number): boolean {
  if (n === 1) return !!selectedTeamId.value
  if (n === 2) return isDateValid.value && !kitBlocked.value
  return true
}

function reset() {
  step.value = 1
  selectedTeamId.value = null
  venueChoice.value = 'mine'
  matchDateStr.value = ''
  matchTimeStr.value = ''
  message.value = ''
}

watch(
  () => props.modelValue,
  (open) => {
    if (!open) return
    reset()
    loadEligibleTeams()
  },
)

function goBack() {
  step.value = Math.max(1, step.value - 1)
}

function close() {
  emit('update:modelValue', false)
}

function submit() {
  if (!selectedTeam.value || !homeTeam.value || !matchDateTime.value) return
  emit('submit', {
    challengerTeamId: selectedTeam.value.id,
    challengedTeamId: props.rivalTeam.id,
    venueTeamId: homeTeam.value.id,
    matchDate: matchDateTime.value.toISOString(),
    message: message.value.trim() || undefined,
  })
}

function goNext() {
  if (!stepIsValid(step.value)) return
  if (step.value >= TOTAL_STEPS) {
    submit()
    return
  }
  step.value += 1
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="560"
    persistent
    scrollable
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="fp-card fp-modal-card">
      <div class="fp-wizard-head">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="fp-wizard-eyebrow">{{ t('challenges.wizard.stepOf', { step, total: TOTAL_STEPS }) }}</p>
            <h3 class="fp-wizard-title">{{ stepTitle }}</h3>
          </div>
          <button type="button" class="fp-wizard-close" :aria-label="t('challenges.wizard.cancel')" @click="close">
            <v-icon :icon="mdiClose" size="16" />
          </button>
        </div>
        <div class="fp-stepper">
          <div
            v-for="n in TOTAL_STEPS"
            :key="n"
            class="fp-stepper-seg"
            :class="{ 'fp-stepper-seg--on': n <= step }"
          />
        </div>
      </div>

      <v-card-text class="fp-wizard-body">
        <!-- Step 1: pick which of my teams challenges -->
        <template v-if="step === 1">
          <p class="fp-hint mb-4">{{ t('challenges.wizard.teamHint') }}</p>

          <v-progress-circular v-if="loadingTeams" indeterminate color="primary" class="d-block mx-auto my-8" />

          <p v-else-if="eligibleTeams.length === 0" class="fp-error">
            {{ t('challenges.wizard.noEligibleTeams') }}
          </p>

          <div v-else class="cw-team-list">
            <button
              v-for="team in eligibleTeams"
              :key="team.id"
              type="button"
              class="cw-team-option"
              :class="{ 'cw-team-option--on': selectedTeamId === team.id }"
              @click="selectedTeamId = team.id"
            >
              <TeamCrest :team="team" :size="40" />
              <div class="cw-team-option-info">
                <div class="cw-team-option-head">
                  <span class="cw-team-option-name">{{ team.name }}</span>
                  <span class="cw-team-option-role">{{ t(`profile.team.memberRoles.${roleFor(team.id)}`) }}</span>
                </div>
                <div class="cw-team-option-chips">
                  <v-chip size="x-small" variant="tonal" :color="MODALITY_COLOR[team.type]">
                    {{ t(`profile.team.enums.${team.type}`) }}
                  </v-chip>
                  <v-chip size="x-small" variant="tonal" :color="AGE_CATEGORY_COLOR[team.category]">
                    {{ t(`profile.team.enums.${team.category}`) }}
                  </v-chip>
                  <v-chip size="x-small" variant="tonal" :color="DIVISION_COLOR[team.division]">
                    {{ t(`profile.team.enums.${team.division}`) }}
                  </v-chip>
                </div>
              </div>
            </button>
          </div>
        </template>

        <!-- Step 2: venue, date, time - kit is resolved automatically -->
        <template v-else-if="step === 2">
          <div class="cw-vs mb-5">
            <div class="cw-vs-side">
              <TeamCrest v-if="selectedTeam" :team="selectedTeam" :size="48" />
              <span>{{ selectedTeam?.name }}</span>
            </div>
            <span class="cw-vs-label">VS</span>
            <div class="cw-vs-side">
              <TeamCrest :team="rivalTeam" :size="48" />
              <span>{{ rivalTeam.name }}</span>
            </div>
          </div>

          <FlatField :label="t('challenges.wizard.venueLabel')" class="mb-4">
            <div class="cw-venue-choice">
              <button
                type="button"
                class="fp-btn"
                :class="venueChoice === 'mine' ? 'fp-btn-solid' : 'fp-btn-outline'"
                @click="venueChoice = 'mine'"
              >
                {{ t('challenges.wizard.venueMine') }}
              </button>
              <button
                type="button"
                class="fp-btn"
                :class="venueChoice === 'rival' ? 'fp-btn-solid' : 'fp-btn-outline'"
                @click="venueChoice = 'rival'"
              >
                {{ t('challenges.wizard.venueRival', { team: rivalTeam.name }) }}
              </button>
            </div>
            <span class="fp-hint">
              {{ homeTeam && hasVenue(homeTeam) ? homeTeam.venueName : t('challenges.wizard.venueUndefined') }}
            </span>
          </FlatField>

          <v-row dense>
            <v-col cols="6">
              <FlatField :label="t('challenges.wizard.dateLabel')">
                <input v-model="matchDateStr" type="date" class="fp-input" :min="todayStr" />
              </FlatField>
            </v-col>
            <v-col cols="6">
              <FlatField :label="t('challenges.wizard.timeLabel')">
                <input v-model="matchTimeStr" type="time" class="fp-input" />
              </FlatField>
            </v-col>
          </v-row>
          <span v-if="(matchDateStr || matchTimeStr) && !isDateValid" class="fp-error d-block mb-4">
            {{ t('challenges.wizard.dateRequired') }}
          </span>

          <div class="cw-kits mt-2">
            <div class="cw-kit-col">
              <span class="cw-kit-tag">{{ t('challenges.wizard.kitHome') }}</span>
              <KitPreview
                v-if="homeKitPreview"
                :pattern="homeKitPreview.kitPattern"
                :primary="homeKitPreview.colorPrimary"
                :secondary="homeKitPreview.colorSecondary"
                :shorts="homeKitPreview.shortsColor"
                :size="72"
              />
              <span v-else class="fp-error">{{ t('challenges.wizard.noHomeKit') }}</span>
            </div>
            <div class="cw-kit-col">
              <span class="cw-kit-tag">{{ t('challenges.wizard.kitAway') }}</span>
              <KitPreview
                v-if="awayKitPreview"
                :pattern="awayKitPreview.kitPattern"
                :primary="awayKitPreview.colorPrimary"
                :secondary="awayKitPreview.colorSecondary"
                :shorts="awayKitPreview.shortsColor"
                :size="72"
              />
              <span v-else-if="homeKitPreview && awayTeam" class="fp-error">
                {{ t('challenges.wizard.noValidAwayKit', { team: awayTeam.name }) }}
              </span>
            </div>
          </div>
          <p class="fp-hint mt-2">{{ t('challenges.wizard.kitAutoHint') }}</p>
        </template>

        <!-- Step 3: summary -->
        <template v-else>
          <div class="cw-vs mb-5">
            <div class="cw-vs-side">
              <TeamCrest v-if="homeTeam" :team="homeTeam" :size="48" />
              <span>{{ homeTeam?.name }}</span>
              <span class="cw-kit-tag">{{ t('challenges.wizard.kitHome') }}</span>
            </div>
            <span class="cw-vs-label">VS</span>
            <div class="cw-vs-side">
              <TeamCrest v-if="awayTeam" :team="awayTeam" :size="48" />
              <span>{{ awayTeam?.name }}</span>
              <span class="cw-kit-tag">{{ t('challenges.wizard.kitAway') }}</span>
            </div>
          </div>

          <div class="cw-summary-row">
            <v-icon :icon="mdiCalendarOutline" size="18" />
            <span>{{ matchDateTime?.toLocaleDateString() }}</span>
          </div>
          <div class="cw-summary-row">
            <v-icon :icon="mdiClockOutline" size="18" />
            <span>{{ matchDateTime?.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}</span>
          </div>
          <div class="cw-summary-row">
            <v-icon :icon="mdiMapMarkerOutline" size="18" />
            <span>{{ homeTeam && hasVenue(homeTeam) ? homeTeam.venueName : t('challenges.wizard.venueUndefined') }}</span>
          </div>

          <div class="cw-kits mt-4">
            <div class="cw-kit-col">
              <KitPreview
                v-if="homeKitPreview"
                :pattern="homeKitPreview.kitPattern"
                :primary="homeKitPreview.colorPrimary"
                :secondary="homeKitPreview.colorSecondary"
                :shorts="homeKitPreview.shortsColor"
                :size="72"
              />
            </div>
            <div class="cw-kit-col">
              <KitPreview
                v-if="awayKitPreview"
                :pattern="awayKitPreview.kitPattern"
                :primary="awayKitPreview.colorPrimary"
                :secondary="awayKitPreview.colorSecondary"
                :shorts="awayKitPreview.shortsColor"
                :size="72"
              />
            </div>
          </div>

          <FlatField :label="t('challenges.wizard.messageLabel')" class="mt-4">
            <textarea
              v-model="message"
              class="fp-input"
              rows="3"
              maxlength="500"
              :placeholder="t('challenges.wizard.messagePlaceholder')"
            />
          </FlatField>
        </template>
      </v-card-text>

      <div class="fp-wizard-foot">
        <button v-if="step === 1" type="button" class="fp-btn fp-btn-text" @click="close">
          {{ t('challenges.wizard.cancel') }}
        </button>
        <button v-else type="button" class="fp-btn fp-btn-outline" @click="goBack">
          <v-icon :icon="mdiChevronLeft" size="16" />
          {{ t('challenges.wizard.back') }}
        </button>
        <button type="button" class="fp-btn fp-btn-solid" :disabled="loading || !stepIsValid(step)" @click="goNext">
          <v-progress-circular v-if="loading" indeterminate size="16" width="2" color="white" />
          <template v-else>
            {{ step === TOTAL_STEPS ? t('challenges.wizard.send') : t('challenges.wizard.next') }}
            <v-icon :icon="step === TOTAL_STEPS ? mdiCheck : mdiChevronRight" size="16" />
          </template>
        </button>
      </div>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.cw-team-list {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.cw-team-option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 0.85rem;
  border: 1.5px solid rgba(var(--v-theme-on-surface), 0.14);
  border-radius: 0.75rem;
  background: none;
  text-align: left;
  cursor: pointer;
}

.cw-team-option--on {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.06);
}

.cw-team-option-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.cw-team-option-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.cw-team-option-name {
  font-weight: 600;
  font-size: 0.9375rem;
}

.cw-team-option-role {
  font-size: 0.75rem;
  font-weight: 600;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
  flex-shrink: 0;
}

.cw-team-option-chips {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
}

.cw-vs {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.25rem;
}

.cw-vs-side {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.35rem;
  font-weight: 600;
  font-size: 0.875rem;
  text-align: center;
  flex: 1;
  min-width: 0;
}

.cw-vs-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.cw-venue-choice {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-bottom: 0.35rem;
}

.cw-kits {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
}

.cw-kit-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.cw-kit-tag {
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  background: rgba(var(--v-theme-primary), 0.1);
  color: rgb(var(--v-theme-primary));
}

.cw-summary-row {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.9375rem;
  margin-bottom: 0.5rem;
}
</style>
