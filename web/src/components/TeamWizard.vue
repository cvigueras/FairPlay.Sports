<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiCheck, mdiChevronLeft, mdiChevronRight, mdiClose, mdiImageOutline } from '@mdi/js'
import FlatField from '@/components/FlatField.vue'
import KitPreview from '@/components/KitPreview.vue'
import KitSwatch from '@/components/KitSwatch.vue'
import RolePills from '@/components/RolePills.vue'
import { KIT_COLOR_PALETTE } from '@/lib/kitColors'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  KIT_PATTERNS,
  PITCH_SURFACES,
  type AgeCategory,
  type CreateTeamPayload,
  type Division,
  type FootballType,
  type KitPattern,
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
  type: null as FootballType | null,
  division: null as Division | null,
  category: null as AgeCategory | null,
  shortName: '',
  foundedYear: null as number | null,
  venueName: '',
  venueAddress: '',
  venueSurface: null as PitchSurface | null,
  venueMapsUrl: '',
  colorPrimary: '#16a34a',
  colorSecondary: '#ffffff',
  shortsColor: '#1e293b',
  kitPattern: 'Plain' as KitPattern,
  alternateColorPrimary: '#0f172a',
  alternateColorSecondary: '#ffffff',
  alternateShortsColor: '#1e293b',
  alternateKitPattern: 'Plain' as KitPattern,
  noSecondKit: false,
  contactEmail: '',
  contactPhone: '',
  website: '',
})

const crest = ref<File | null>(null)
const crestPreviewUrl = ref('')
const errors = reactive<Record<string, string>>({})

/** Which of the two shirt-colour slots the palette strip underneath is
 *  currently editing - one shared strip per kit, not a picker per colour. */
const kitColorSlot = ref<'primary' | 'secondary'>('primary')
const alternateKitColorSlot = ref<'primary' | 'secondary'>('primary')

function pickKitColor(color: string) {
  if (kitColorSlot.value === 'primary') model.colorPrimary = color
  else model.colorSecondary = color
}

function pickAlternateKitColor(color: string) {
  if (alternateKitColorSlot.value === 'primary') model.alternateColorPrimary = color
  else model.alternateColorSecondary = color
}

function resetForm() {
  step.value = 1
  isEdit.value = false
  kitColorSlot.value = 'primary'
  alternateKitColorSlot.value = 'primary'
  Object.assign(model, {
    name: '',
    role: null,
    coach: '',
    city: '',
    type: null,
    division: null,
    category: null,
    shortName: '',
    foundedYear: null,
    venueName: '',
    venueAddress: '',
    venueSurface: null,
    venueMapsUrl: '',
    colorPrimary: '#16a34a',
    colorSecondary: '#ffffff',
    shortsColor: '#1e293b',
    kitPattern: 'Plain',
    alternateColorPrimary: '#0f172a',
    alternateColorSecondary: '#ffffff',
    alternateShortsColor: '#1e293b',
    alternateKitPattern: 'Plain',
    noSecondKit: false,
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
  kitColorSlot.value = 'primary'
  alternateKitColorSlot.value = 'primary'
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
    colorSecondary: team.kitPattern === 'Plain' ? '#ffffff' : team.colorSecondary || '#ffffff',
    shortsColor: team.shortsColor || '#1e293b',
    kitPattern: team.kitPattern ?? 'Plain',
    alternateColorPrimary: team.alternateColorPrimary || '#0f172a',
    alternateColorSecondary:
      team.alternateKitPattern === 'Plain' ? '#ffffff' : team.alternateColorSecondary || '#ffffff',
    alternateShortsColor: team.alternateShortsColor || '#1e293b',
    alternateKitPattern: team.alternateKitPattern ?? 'Plain',
    noSecondKit: !team.alternateColorPrimary && !team.alternateColorSecondary,
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

/** A plain shirt has no secondary colour to show, so the picker hides it and
 *  parks it at white - the neutral default the other patterns start from. */
watch(
  () => model.kitPattern,
  (pattern) => {
    if (pattern !== 'Plain') return
    model.colorSecondary = '#ffffff'
    if (kitColorSlot.value === 'secondary') kitColorSlot.value = 'primary'
  },
)
watch(
  () => model.alternateKitPattern,
  (pattern) => {
    if (pattern !== 'Plain') return
    model.alternateColorSecondary = '#ffffff'
    if (alternateKitColorSlot.value === 'secondary') alternateKitColorSlot.value = 'primary'
  },
)

const enumItems = <T extends string>(values: readonly T[], prefix: string) =>
  values.map((value) => ({ value, title: t(`${prefix}.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES, 'profile.team.enums'))
const divisionItems = computed(() => enumItems(DIVISIONS, 'profile.team.enums'))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES, 'profile.team.enums'))
const surfaceItems = computed(() => enumItems(PITCH_SURFACES, 'profile.team.surfaces'))

const STEP_TITLE_KEYS = ['stepDetails', 'stepVenue', 'stepKit', 'stepKitSecondary', 'stepContact']
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
const STEP_FIELDS: Record<number, string[]> = {
  1: ['name', 'role', 'coach', 'city', 'crest', 'type', 'division', 'category'],
  2: ['venueName', 'venueAddress', 'venueSurface', 'venueMapsUrl', 'foundedYear'],
  3: ['colorPrimary', 'colorSecondary', 'shortsColor', 'kitPattern'],
  4: ['alternateColorPrimary', 'alternateColorSecondary', 'alternateShortsColor', 'alternateKitPattern'],
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
  if (!model.type) errors.type = required
  if (!model.division) errors.division = required
  if (!model.category) errors.category = required

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

  if (!model.colorPrimary.trim()) errors.colorPrimary = required
  if (!model.colorSecondary.trim()) errors.colorSecondary = required
  if (!model.shortsColor.trim()) errors.shortsColor = required

  if (!model.noSecondKit) {
    if (!model.alternateColorPrimary.trim()) errors.alternateColorPrimary = required
    if (!model.alternateColorSecondary.trim()) errors.alternateColorSecondary = required
    if (!model.alternateShortsColor.trim()) errors.alternateShortsColor = required
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
    type: model.type!,
    division: model.division!,
    category: model.category!,
    shortName: trimmedOrUndefined(model.shortName),
    foundedYear: model.foundedYear ?? undefined,
    venueName: trimmedOrUndefined(model.venueName),
    venueAddress: trimmedOrUndefined(model.venueAddress),
    venueSurface: model.venueSurface ?? undefined,
    venueMapsUrl: trimmedOrUndefined(model.venueMapsUrl),
    colorPrimary: trimmedOrUndefined(model.colorPrimary),
    colorSecondary: trimmedOrUndefined(model.colorSecondary),
    shortsColor: trimmedOrUndefined(model.shortsColor),
    kitPattern: model.kitPattern,
    alternateColorPrimary: model.noSecondKit ? undefined : trimmedOrUndefined(model.alternateColorPrimary),
    alternateColorSecondary: model.noSecondKit ? undefined : trimmedOrUndefined(model.alternateColorSecondary),
    alternateShortsColor: model.noSecondKit ? undefined : trimmedOrUndefined(model.alternateShortsColor),
    alternateKitPattern: model.noSecondKit ? undefined : model.alternateKitPattern,
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
    max-width="680"
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
              <strong>
                {{ t('profile.team.crest') }}<span v-if="!isEdit" class="fp-required-mark">*</span>
              </strong>
              <span>{{ t('profile.team.wizard.crestHint') }}</span>
            </span>
          </div>
          <div v-if="errors.crest" class="fp-error mb-4">{{ errors.crest }}</div>
          <div v-else class="mb-4" />

          <FlatField
            :label="t('profile.team.wizard.nameLabel')"
            :hint="isEdit ? '' : t('profile.team.wizard.nameHint')"
            :error="errors.name"
            required
            class="mb-4"
          >
            <input v-model="model.name" class="fp-input" :class="{ 'fp-invalid': errors.name }" type="text" />
          </FlatField>

          <FlatField v-if="!isEdit" :label="t('profile.team.memberRole')" required class="mb-3">
            <RolePills v-model="model.role" />
            <span v-if="errors.role" class="fp-error">{{ errors.role }}</span>
          </FlatField>

          <v-row dense>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.coach')" :error="errors.coach" required>
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
              <FlatField :label="t('profile.team.city')" :error="errors.city" required>
                <input v-model="model.city" class="fp-input" :class="{ 'fp-invalid': errors.city }" type="text" />
              </FlatField>
            </v-col>
          </v-row>

          <v-row dense class="mt-1">
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.type')" :error="errors.type" required>
                <select v-model="model.type" class="fp-select" :class="{ 'fp-invalid': errors.type }">
                  <option :value="null">{{ t('profile.team.selectType') }}</option>
                  <option v-for="opt in typeItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
            <v-col cols="12" sm="6">
              <FlatField :label="t('profile.team.division')" :error="errors.division" required>
                <select v-model="model.division" class="fp-select" :class="{ 'fp-invalid': errors.division }">
                  <option :value="null">{{ t('profile.team.selectDivision') }}</option>
                  <option v-for="opt in divisionItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
            <v-col cols="12">
              <FlatField :label="t('profile.team.category')" :error="errors.category" required>
                <select v-model="model.category" class="fp-select" :class="{ 'fp-invalid': errors.category }">
                  <option :value="null">{{ t('profile.team.selectCategory') }}</option>
                  <option v-for="opt in categoryItems" :key="opt.value" :value="opt.value">{{ opt.title }}</option>
                </select>
              </FlatField>
            </v-col>
          </v-row>
        </template>

        <!-- Step 2: Campo, con la ficha del club debajo -->
        <template v-else-if="step === 2">
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

          <h3 class="fp-wizard-title mt-4 mb-3">{{ t('profile.team.wizard.stepClub') }}</h3>
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

        <!-- Step 3: 1ª equipación -->
        <template v-else-if="step === 3">
          <div class="fp-kit-layout">
            <div class="fp-kit-preview-col">
              <div class="fp-kit-preview-card">
                <KitPreview
                  :pattern="model.kitPattern"
                  :primary="model.colorPrimary"
                  :secondary="model.colorSecondary"
                  :shorts="model.shortsColor"
                  :size="130"
                />
              </div>
              <div class="fp-kit-preview-caption">{{ t('profile.team.kitPreviewCaption') }}</div>
            </div>

            <div class="fp-kit-controls">
              <div>
                <div class="fp-kit-section-title">
                  {{ t('profile.team.shirtColorsGroup') }}<span class="fp-required-mark">*</span>
                </div>
                <div class="fp-kit-swatch-row">
                  <button
                    type="button"
                    class="fp-kit-swatch-slot"
                    :class="{ 'fp-kit-swatch-slot--active': kitColorSlot === 'primary' }"
                    @click="kitColorSlot = 'primary'"
                  >
                    <span class="fp-kit-swatch-dot" :style="{ background: model.colorPrimary }" />
                    {{ t('profile.team.colorPrimary') }}
                  </button>
                  <button
                    v-if="model.kitPattern !== 'Plain'"
                    type="button"
                    class="fp-kit-swatch-slot"
                    :class="{ 'fp-kit-swatch-slot--active': kitColorSlot === 'secondary' }"
                    @click="kitColorSlot = 'secondary'"
                  >
                    <span class="fp-kit-swatch-dot" :style="{ background: model.colorSecondary }" />
                    {{ t('profile.team.colorSecondary') }}
                  </button>
                </div>
                <div class="fp-kit-palette">
                  <div class="fp-kit-palette-row">
                    <button
                      v-for="c in KIT_COLOR_PALETTE"
                      :key="c.value"
                      type="button"
                      class="fp-kit-palette-swatch"
                      :style="{ background: c.value }"
                      :disabled="
                        model.kitPattern !== 'Plain' &&
                        (kitColorSlot === 'primary' ? model.colorSecondary : model.colorPrimary).toLowerCase() ===
                          c.value.toLowerCase()
                      "
                      :aria-label="t(`profile.team.colorNames.${c.labelKey}`)"
                      :title="t(`profile.team.colorNames.${c.labelKey}`)"
                      @click="pickKitColor(c.value)"
                    />
                  </div>
                </div>
                <span v-if="errors.colorPrimary || errors.colorSecondary" class="fp-error">
                  {{ errors.colorPrimary || errors.colorSecondary }}
                </span>
              </div>

              <div>
                <div class="fp-kit-section-title">{{ t('profile.team.kitPattern') }}</div>
                <div class="fp-kit-patterns">
                  <button
                    v-for="pattern in KIT_PATTERNS"
                    :key="pattern"
                    type="button"
                    class="fp-kit-pattern"
                    :class="{ 'fp-kit-pattern--selected': model.kitPattern === pattern }"
                    :aria-label="t(`profile.team.kitPatterns.${pattern}`)"
                    :title="t(`profile.team.kitPatterns.${pattern}`)"
                    @click="model.kitPattern = pattern"
                  >
                    <KitSwatch :pattern="pattern" :primary="model.colorPrimary" :secondary="model.colorSecondary" />
                  </button>
                </div>
              </div>

              <div>
                <div class="fp-kit-section-title">
                  {{ t('profile.team.shortsColor') }}<span class="fp-required-mark">*</span>
                </div>
                <div class="fp-kit-section-hint">{{ t('profile.team.shortsColorHint') }}</div>
                <div class="fp-kit-swatch-row">
                  <span class="fp-kit-swatch-slot fp-kit-swatch-slot--single">
                    <span class="fp-kit-swatch-dot" :style="{ background: model.shortsColor }" />
                    {{ t('profile.team.shortsColor') }}
                  </span>
                </div>
                <div class="fp-kit-palette">
                  <div class="fp-kit-palette-row">
                    <button
                      v-for="c in KIT_COLOR_PALETTE"
                      :key="c.value"
                      type="button"
                      class="fp-kit-palette-swatch"
                      :style="{ background: c.value }"
                      :aria-label="t(`profile.team.colorNames.${c.labelKey}`)"
                      :title="t(`profile.team.colorNames.${c.labelKey}`)"
                      @click="model.shortsColor = c.value"
                    />
                  </div>
                </div>
                <span v-if="errors.shortsColor" class="fp-error">{{ errors.shortsColor }}</span>
              </div>
            </div>
          </div>
        </template>

        <!-- Step 4: 2ª equipación -->
        <template v-else-if="step === 4">
          <div class="fp-kit-toggle-row">
            <div>
              <div class="fp-kit-toggle-title">{{ t('profile.team.hasSecondKitQuestion') }}</div>
              <div class="fp-kit-toggle-hint">{{ t('profile.team.hasSecondKitHint') }}</div>
            </div>
            <div class="fp-kit-toggle-pill">
              <button
                type="button"
                :class="{ 'fp-kit-toggle-pill--active': !model.noSecondKit }"
                @click="model.noSecondKit = false"
              >
                {{ t('profile.team.yes') }}
              </button>
              <button
                type="button"
                :class="{ 'fp-kit-toggle-pill--active': model.noSecondKit }"
                @click="model.noSecondKit = true"
              >
                {{ t('profile.team.no') }}
              </button>
            </div>
          </div>

          <div v-if="!model.noSecondKit" class="fp-kit-layout">
            <div class="fp-kit-preview-col">
              <div class="fp-kit-preview-card">
                <KitPreview
                  :pattern="model.alternateKitPattern"
                  :primary="model.alternateColorPrimary"
                  :secondary="model.alternateColorSecondary"
                  :shorts="model.alternateShortsColor"
                  :size="130"
                />
              </div>
              <div class="fp-kit-preview-caption">{{ t('profile.team.kitPreviewCaption') }}</div>
            </div>

            <div class="fp-kit-controls">
              <div>
                <div class="fp-kit-section-title">{{ t('profile.team.shirtColorsGroup') }}</div>
                <div class="fp-kit-swatch-row">
                  <button
                    type="button"
                    class="fp-kit-swatch-slot"
                    :class="{ 'fp-kit-swatch-slot--active': alternateKitColorSlot === 'primary' }"
                    @click="alternateKitColorSlot = 'primary'"
                  >
                    <span class="fp-kit-swatch-dot" :style="{ background: model.alternateColorPrimary }" />
                    {{ t('profile.team.colorPrimary') }}
                  </button>
                  <button
                    v-if="model.alternateKitPattern !== 'Plain'"
                    type="button"
                    class="fp-kit-swatch-slot"
                    :class="{ 'fp-kit-swatch-slot--active': alternateKitColorSlot === 'secondary' }"
                    @click="alternateKitColorSlot = 'secondary'"
                  >
                    <span class="fp-kit-swatch-dot" :style="{ background: model.alternateColorSecondary }" />
                    {{ t('profile.team.colorSecondary') }}
                  </button>
                </div>
                <div class="fp-kit-palette">
                  <div class="fp-kit-palette-row">
                    <button
                      v-for="c in KIT_COLOR_PALETTE"
                      :key="c.value"
                      type="button"
                      class="fp-kit-palette-swatch"
                      :style="{ background: c.value }"
                      :disabled="
                        model.alternateKitPattern !== 'Plain' &&
                        (alternateKitColorSlot === 'primary'
                          ? model.alternateColorSecondary
                          : model.alternateColorPrimary
                        ).toLowerCase() === c.value.toLowerCase()
                      "
                      :aria-label="t(`profile.team.colorNames.${c.labelKey}`)"
                      :title="t(`profile.team.colorNames.${c.labelKey}`)"
                      @click="pickAlternateKitColor(c.value)"
                    />
                  </div>
                </div>
                <span v-if="errors.alternateColorPrimary || errors.alternateColorSecondary" class="fp-error">
                  {{ errors.alternateColorPrimary || errors.alternateColorSecondary }}
                </span>
              </div>

              <div>
                <div class="fp-kit-section-title">{{ t('profile.team.kitPattern') }}</div>
                <div class="fp-kit-patterns">
                  <button
                    v-for="pattern in KIT_PATTERNS"
                    :key="pattern"
                    type="button"
                    class="fp-kit-pattern"
                    :class="{ 'fp-kit-pattern--selected': model.alternateKitPattern === pattern }"
                    :aria-label="t(`profile.team.kitPatterns.${pattern}`)"
                    :title="t(`profile.team.kitPatterns.${pattern}`)"
                    @click="model.alternateKitPattern = pattern"
                  >
                    <KitSwatch
                      :pattern="pattern"
                      :primary="model.alternateColorPrimary"
                      :secondary="model.alternateColorSecondary"
                    />
                  </button>
                </div>
              </div>

              <div>
                <div class="fp-kit-section-title">{{ t('profile.team.shortsColor') }}</div>
                <div class="fp-kit-section-hint">{{ t('profile.team.shortsColorHint') }}</div>
                <div class="fp-kit-swatch-row">
                  <span class="fp-kit-swatch-slot fp-kit-swatch-slot--single">
                    <span class="fp-kit-swatch-dot" :style="{ background: model.alternateShortsColor }" />
                    {{ t('profile.team.shortsColor') }}
                  </span>
                </div>
                <div class="fp-kit-palette">
                  <div class="fp-kit-palette-row">
                    <button
                      v-for="c in KIT_COLOR_PALETTE"
                      :key="c.value"
                      type="button"
                      class="fp-kit-palette-swatch"
                      :style="{ background: c.value }"
                      :aria-label="t(`profile.team.colorNames.${c.labelKey}`)"
                      :title="t(`profile.team.colorNames.${c.labelKey}`)"
                      @click="model.alternateShortsColor = c.value"
                    />
                  </div>
                </div>
                <span v-if="errors.alternateShortsColor" class="fp-error">{{ errors.alternateShortsColor }}</span>
              </div>
            </div>
          </div>

          <div v-else class="fp-kit-empty">
            <div class="fp-kit-empty-icon">
              <KitSwatch pattern="Plain" primary="#cbd5e1" secondary="#cbd5e1" :size="34" />
            </div>
            <div class="fp-kit-empty-title">{{ t('profile.team.noSecondKitTitle') }}</div>
            <div class="fp-kit-empty-text">{{ t('profile.team.noSecondKitHint') }}</div>
          </div>
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
.fp-kit-patterns {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.fp-kit-pattern {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  padding: 6px;
  border-radius: 12px;
  border: 1.5px solid #e2e8f0;
  background: rgb(var(--v-theme-surface));
  cursor: pointer;
}

.fp-kit-pattern:hover {
  border-color: #94a3b8;
}

.fp-kit-pattern--selected {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.08);
  color: rgb(var(--v-theme-primary-darken-1));
}

.fp-kit-pattern svg {
  border-radius: 6px;
}

.fp-kit-layout {
  display: grid;
  grid-template-columns: 190px 1fr;
  gap: 24px;
  align-items: start;
}

.fp-kit-preview-col {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.fp-kit-preview-card {
  min-height: 220px;
  border-radius: 16px;
  border: 1.5px solid #e2e8f0;
  background: radial-gradient(circle at 50% 18%, rgb(var(--v-theme-surface)) 0%, rgb(var(--v-theme-background)) 74%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
}

.fp-kit-preview-caption {
  text-align: center;
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
}

.fp-kit-controls {
  display: flex;
  flex-direction: column;
  gap: 20px;
  min-width: 0;
}

.fp-kit-section-title {
  font-size: 13px;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
  margin-bottom: 10px;
}

.fp-kit-section-hint {
  font-size: 11px;
  color: #64748b;
  margin-top: -6px;
  margin-bottom: 10px;
}

.fp-kit-swatch-row {
  display: flex;
  gap: 10px;
}

.fp-kit-swatch-slot {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 12px;
  border: 1.5px solid #e2e8f0;
  background: rgb(var(--v-theme-surface));
  cursor: pointer;
  font-size: 12px;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
}

.fp-kit-swatch-slot--single {
  cursor: default;
  width: 100%;
}

.fp-kit-swatch-slot--active {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.08);
}

.fp-kit-swatch-dot {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  border: 1px solid rgba(15, 23, 42, 0.15);
  box-shadow: inset 0 0 0 3px rgb(var(--v-theme-surface));
  flex-shrink: 0;
}

.fp-kit-palette {
  margin-top: 10px;
  padding: 10px;
  border-radius: 12px;
  background: rgb(var(--v-theme-background));
  border: 1px solid #e2e8f0;
}

.fp-kit-palette-row {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.fp-kit-palette-swatch {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  border: 1px solid rgba(15, 23, 42, 0.15);
  cursor: pointer;
  padding: 0;
}

.fp-kit-palette-swatch:disabled {
  opacity: 0.25;
  cursor: not-allowed;
}

.fp-kit-toggle-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 16px;
  border-radius: 14px;
  background: rgb(var(--v-theme-background));
  border: 1px solid #e2e8f0;
  margin-bottom: 20px;
}

.fp-kit-toggle-title {
  font-size: 13px;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
}

.fp-kit-toggle-hint {
  font-size: 11px;
  color: #64748b;
  margin-top: 2px;
}

.fp-kit-toggle-pill {
  display: flex;
  gap: 4px;
  background: #e2e8f0;
  padding: 4px;
  border-radius: 999px;
  flex-shrink: 0;
}

.fp-kit-toggle-pill button {
  padding: 6px 16px;
  border-radius: 999px;
  border: none;
  background: transparent;
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
}

.fp-kit-toggle-pill button.fp-kit-toggle-pill--active {
  background: rgb(var(--v-theme-primary));
  color: #fff;
}

.fp-kit-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 32px 16px;
  text-align: center;
}

.fp-kit-empty-icon {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: rgb(var(--v-theme-background));
  display: flex;
  align-items: center;
  justify-content: center;
}

.fp-kit-empty-title {
  font-size: 15px;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
  max-width: 340px;
}

.fp-kit-empty-text {
  font-size: 12px;
  color: #64748b;
  max-width: 320px;
}
</style>
