<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiCheck, mdiChevronLeft, mdiChevronRight, mdiClose, mdiImageOutline } from '@mdi/js'
import FlatField from '@/components/FlatField.vue'
import RolePills from '@/components/RolePills.vue'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  PITCH_SURFACES,
  type AgeCategory,
  type CreateTeamPayload,
  type Division,
  type FootballType,
  type PitchSurface,
  type Team,
  type TeamMemberRole,
} from '@/types/team'

const props = withDefaults(
  defineProps<{ modelValue: boolean; loading?: boolean; initial?: Team | null }>(),
  { loading: false, initial: null },
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (
    e: 'submit',
    value: { payload: CreateTeamPayload; role: TeamMemberRole; displayName: string; crest: File | null },
  ): void
  (e: 'update', value: { payload: CreateTeamPayload; crest: File | null }): void
}>()

const { t } = useI18n()
const auth = useAuthStore()

/** Editing an existing team reuses this same wizard, pre-filled from
 *  `initial`, instead of founding a new one - so no member role to pick and
 *  no crest required (the team may already have one). Set only when the
 *  dialog opens (not a computed off `props.initial`) so it doesn't flip
 *  back to "create" mid-way through the closing transition, once the
 *  parent clears `initial` right after a successful save. */
const isEdit = ref(false)

const TOTAL_STEPS = 5
const step = ref(1)

const model = reactive({
  name: '',
  role: null as TeamMemberRole | null,
  coach: '',
  city: '',
  type: 'Futsal' as FootballType,
  division: 'First' as Division,
  category: 'Alevines' as AgeCategory,
  shortName: '',
  foundedYear: null as number | null,
  venueName: '',
  venueAddress: '',
  venueSurface: null as PitchSurface | null,
  venueMapsUrl: '',
  colorPrimary: '#16a34a',
  colorSecondary: '#ffffff',
  contactEmail: '',
  contactPhone: '',
  website: '',
})

const crest = ref<File | null>(null)
const crestPreviewUrl = ref('')
const errors = reactive<Record<string, string>>({})

function resetForm() {
  step.value = 1
  isEdit.value = false
  Object.assign(model, {
    name: '',
    role: null,
    coach: '',
    city: '',
    type: 'Futsal',
    division: 'First',
    category: 'Alevines',
    shortName: '',
    foundedYear: null,
    venueName: '',
    venueAddress: '',
    venueSurface: null,
    venueMapsUrl: '',
    colorPrimary: '#16a34a',
    colorSecondary: '#ffffff',
    contactEmail: '',
    contactPhone: '',
    website: '',
  })
  crest.value = null
  crestPreviewUrl.value = ''
  for (const key of Object.keys(errors)) delete errors[key]
}

function applyInitial(team: Team) {
  step.value = 1
  isEdit.value = true
  Object.assign(model, {
    name: team.name,
    role: null,
    coach: team.coach,
    city: team.city,
    type: team.type,
    division: team.division,
    category: team.category,
    shortName: team.shortName ?? '',
    foundedYear: team.foundedYear ?? null,
    venueName: team.venueName ?? '',
    venueAddress: team.venueAddress ?? '',
    venueSurface: team.venueSurface ?? null,
    venueMapsUrl: team.venueMapsUrl ?? '',
    colorPrimary: team.colorPrimary || '#16a34a',
    colorSecondary: team.colorSecondary || '#ffffff',
    contactEmail: team.contactEmail ?? '',
    contactPhone: team.contactPhone ?? '',
    website: team.website ?? '',
  })
  crest.value = null
  crestPreviewUrl.value = team.hasCrest ? teamsApi.crestUrl(team.id) : ''
  for (const key of Object.keys(errors)) delete errors[key]
}

watch(
  () => props.modelValue,
  (open) => {
    if (!open) return
    if (props.initial) applyInitial(props.initial)
    else resetForm()
  },
)

/** Picking "Entrenador" as your own role means you ARE the team's coach, so
 *  the coach-name field just mirrors your account name and locks - picking
 *  any other role hands manual control of that field back. */
watch(
  () => model.role,
  (role) => {
    if (role === 'Coach') model.coach = auth.currentUser?.userName ?? ''
  },
)

const enumItems = <T extends string>(values: readonly T[], prefix: string) =>
  values.map((value) => ({ value, title: t(`${prefix}.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES, 'profile.team.enums'))
const divisionItems = computed(() => enumItems(DIVISIONS, 'profile.team.enums'))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES, 'profile.team.enums'))
const surfaceItems = computed(() => enumItems(PITCH_SURFACES, 'profile.team.surfaces'))

const STEP_TITLE_KEYS = ['stepDetails', 'stepClub', 'stepVenue', 'stepKit', 'stepContact']
const stepTitle = computed(() => t(`profile.team.wizard.${STEP_TITLE_KEYS[step.value - 1]}`))

function isHttpUrl(value: string): boolean {
  try {
    const url = new URL(value.trim())
    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}

const anyVenueField = computed(
  () =>
    !!model.venueName.trim() || !!model.venueAddress.trim() || !!model.venueSurface || !!model.venueMapsUrl.trim(),
)
const anyColour = computed(() => !!model.colorPrimary.trim() || !!model.colorSecondary.trim())

const STEP_FIELDS: Record<number, string[]> = {
  1: ['name', 'role', 'coach', 'city', 'crest'],
  2: ['foundedYear'],
  3: ['venueName', 'venueAddress', 'venueSurface', 'venueMapsUrl'],
  4: ['colorPrimary', 'colorSecondary'],
  5: ['website'],
}

function validate(): boolean {
  const required = t('profile.team.required')
  for (const key of Object.keys(errors)) delete errors[key]

  if (!model.name.trim()) errors.name = required
  if (!isEdit.value && !model.role) errors.role = required
  if (!model.coach.trim()) errors.coach = required
  if (!model.city.trim()) errors.city = required
  if (!isEdit.value && !crest.value) errors.crest = t('profile.team.crestRequired')

  if (model.foundedYear != null) {
    const year = model.foundedYear
    const max = new Date().getFullYear()
    if (year < 1850 || year > max) errors.foundedYear = t('profile.team.foundedYearRange', { max })
  }

  if (anyVenueField.value) {
    if (!model.venueName.trim()) errors.venueName = required
    if (!model.venueAddress.trim()) errors.venueAddress = required
    if (!model.venueSurface) errors.venueSurface = required
  }
  if (model.venueMapsUrl.trim() && !isHttpUrl(model.venueMapsUrl)) {
    errors.venueMapsUrl = t('profile.team.urlInvalid')
  }

  if (anyColour.value) {
    if (!model.colorPrimary.trim()) errors.colorPrimary = required
    if (!model.colorSecondary.trim()) errors.colorSecondary = required
  }

  if (model.website.trim() && !isHttpUrl(model.website)) errors.website = t('profile.team.urlInvalid')

  return Object.keys(errors).length === 0
}

function hasStepError(n: number): boolean {
  return (STEP_FIELDS[n] ?? []).some((field) => errors[field])
}

function goBack() {
  step.value = Math.max(1, step.value - 1)
}

function close() {
  emit('update:modelValue', false)
}

function handleCrest(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  crest.value = file
  const reader = new FileReader()
  reader.onload = () => {
    crestPreviewUrl.value = String(reader.result)
  }
  reader.readAsDataURL(file)
}

function trimmedOrUndefined(value: string): string | undefined {
  const trimmed = value.trim()
  return trimmed ? trimmed : undefined
}

function submit() {
  if (!validate()) return
  if (!isEdit.value && !model.role) return
  const name = model.name.trim()
  const payload: CreateTeamPayload = {
    name,
    coach: model.coach.trim(),
    city: model.city.trim(),
    type: model.type,
    division: model.division,
    category: model.category,
    shortName: trimmedOrUndefined(model.shortName),
    foundedYear: model.foundedYear ?? undefined,
    venueName: trimmedOrUndefined(model.venueName),
    venueAddress: trimmedOrUndefined(model.venueAddress),
    venueSurface: model.venueSurface ?? undefined,
    venueMapsUrl: trimmedOrUndefined(model.venueMapsUrl),
    colorPrimary: trimmedOrUndefined(model.colorPrimary),
    colorSecondary: trimmedOrUndefined(model.colorSecondary),
    contactEmail: trimmedOrUndefined(model.contactEmail),
    contactPhone: trimmedOrUndefined(model.contactPhone),
    website: trimmedOrUndefined(model.website),
  }
  if (isEdit.value) emit('update', { payload, crest: crest.value })
  else emit('submit', { payload, role: model.role!, displayName: name, crest: crest.value })
}

function goNext() {
  validate()
  if (hasStepError(step.value)) return
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
    max-width="620"
    persistent
    scrollable
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="fp-card fp-modal-card">
      <div class="fp-wizard-head">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="fp-wizard-eyebrow">{{ t('profile.team.wizard.stepOf', { step, total: TOTAL_STEPS }) }}</p>
            <h3 class="fp-wizard-title">{{ stepTitle }}</h3>
          </div>
          <button type="button" class="fp-wizard-close" :aria-label="t('profile.team.cancel')" @click="close">
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
        <!-- Step 1: team basics + your role -->
        <template v-if="step === 1">
          <div class="fp-crest-drop mb-1">
            <input type="file" accept="image/png,image/jpeg,image/webp,image/svg+xml" @change="handleCrest" />
            <img v-if="crestPreviewUrl" class="fp-crest-thumb" :src="crestPreviewUrl" alt="" />
            <span v-else class="fp-crest-empty"><v-icon :icon="mdiImageOutline" size="24" /></span>
            <span class="fp-crest-copy">
              <strong>{{ t('profile.team.crest') }}</strong>
              <span>{{ t('profile.team.wizard.crestHint') }}</span>
            </span>
          </div>
          <div v-if="errors.crest" class="fp-error mb-4">{{ errors.crest }}</div>
          <div v-else class="mb-4" />

          <FlatField
            :label="t('profile.team.wizard.nameLabel')"
            :hint="isEdit ? '' : t('profile.team.wizard.nameHint')"
            :error="errors.name"
            class="mb-4"
          >
            <input v-model="model.name" class="fp-input" :class="{ 'fp-invalid': errors.name }" type="text" />
          </FlatField>

          <FlatField v-if="!isEdit" :label="t('profile.team.memberRole')" class="mb-3">
            <RolePills v-model="model.role" />
            <span v-if="errors.role" class="fp-error">{{ errors.role }}</span>
          </FlatField>

          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.coach')" :error="errors.coach">
                <input
                  v-model="model.coach"
                  class="fp-input"
                  :class="{ 'fp-invalid': errors.coach }"
                  type="text"
                  :disabled="model.role === 'Coach'"
                />
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.city')" :error="errors.city">
                <input v-model="model.city" class="fp-input" :class="{ 'fp-invalid': errors.city }" type="text" />
              </FlatField>
            </v-col>
          </v-row>

          <v-row dense class="mt-1">
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.type')">
                <select v-model="model.type" class="fp-select">
                  <option v-for="opt in typeItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.division')">
                <select v-model="model.division" class="fp-select">
                  <option v-for="opt in divisionItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
            <v-col cols="12">
              <FlatField :label="t('profile.team.category')">
                <select v-model="model.category" class="fp-select">
                  <option v-for="opt in categoryItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
          </v-row>
        </template>

        <!-- Step 2: Ficha del club -->
        <template v-else-if="step === 2">
          <p class="fp-section-hint">{{ t('profile.team.wizard.clubSheetHint') }}</p>
          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.shortName')">
                <input v-model="model.shortName" class="fp-input" type="text" maxlength="20" />
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.foundedYear')" :error="errors.foundedYear">
                <input
                  v-model.number="model.foundedYear"
                  class="fp-input"
                  :class="{ 'fp-invalid': errors.foundedYear }"
                  type="number"
                />
              </FlatField>
            </v-col>
          </v-row>
        </template>

        <!-- Step 3: Campo -->
        <template v-else-if="step === 3">
          <FlatField :label="t('profile.team.venueName')" :error="errors.venueName" class="mb-3">
            <input
              v-model="model.venueName"
              class="fp-input"
              :class="{ 'fp-invalid': errors.venueName }"
              type="text"
            />
          </FlatField>
          <FlatField :label="t('profile.team.venueAddress')" :error="errors.venueAddress" class="mb-3">
            <input
              v-model="model.venueAddress"
              class="fp-input"
              :class="{ 'fp-invalid': errors.venueAddress }"
              type="text"
            />
          </FlatField>
          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.venueSurface')" :error="errors.venueSurface">
                <select v-model="model.venueSurface" class="fp-select" :class="{ 'fp-invalid': errors.venueSurface }">
                  <option :value="null" />
                  <option v-for="opt in surfaceItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField
                :label="t('profile.team.venueMapsUrl')"
                :hint="t('profile.team.venueMapsUrlHint')"
                :error="errors.venueMapsUrl"
              >
                <input
                  v-model="model.venueMapsUrl"
                  class="fp-input"
                  :class="{ 'fp-invalid': errors.venueMapsUrl }"
                  type="url"
                />
              </FlatField>
            </v-col>
          </v-row>
        </template>

        <!-- Step 4: Equipación -->
        <template v-else-if="step === 4">
          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.colorPrimary')">
                <div class="fp-color-field">
                  <input v-model="model.colorPrimary" type="color" class="color-swatch" />
                  <span class="fp-color-hex">{{ model.colorPrimary }}</span>
                </div>
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.colorSecondary')">
                <div class="fp-color-field">
                  <input v-model="model.colorSecondary" type="color" class="color-swatch" />
                  <span class="fp-color-hex">{{ model.colorSecondary }}</span>
                </div>
              </FlatField>
            </v-col>
          </v-row>
        </template>

        <!-- Step 5: Contacto -->
        <template v-else>
          <FlatField :label="t('profile.team.contactEmail')" class="mb-3">
            <input v-model="model.contactEmail" class="fp-input" type="email" />
          </FlatField>
          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.contactPhone')">
                <input v-model="model.contactPhone" class="fp-input" type="tel" />
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.website')" :error="errors.website">
                <input
                  v-model="model.website"
                  class="fp-input"
                  :class="{ 'fp-invalid': errors.website }"
                  type="url"
                />
              </FlatField>
            </v-col>
          </v-row>
        </template>
      </v-card-text>

      <div class="fp-wizard-foot">
        <button v-if="step === 1" type="button" class="fp-btn fp-btn-text" @click="close">
          {{ t('profile.team.cancel') }}
        </button>
        <button v-else type="button" class="fp-btn fp-btn-outline" @click="goBack">
          <v-icon :icon="mdiChevronLeft" size="16" />
          {{ t('profile.team.wizard.back') }}
        </button>
        <button type="button" class="fp-btn fp-btn-solid" :disabled="loading" @click="goNext">
          <v-progress-circular v-if="loading" indeterminate size="16" width="2" color="white" />
          <template v-else>
            {{
              step === TOTAL_STEPS
                ? isEdit
                  ? t('profile.team.saveChanges')
                  : t('profile.team.wizard.finish')
                : t('profile.team.wizard.next')
            }}
            <v-icon :icon="step === TOTAL_STEPS ? mdiCheck : mdiChevronRight" size="16" />
          </template>
        </button>
      </div>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.color-swatch {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  border: 1.5px solid #e2e8f0;
  padding: 0;
  cursor: pointer;
  background: none;
  flex-shrink: 0;
}
</style>
