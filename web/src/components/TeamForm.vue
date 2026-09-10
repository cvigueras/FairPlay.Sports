<script setup lang="ts">
import { reactive, ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiImageOutline } from '@mdi/js'
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
} from '@/types/team'

const props = withDefaults(
  defineProps<{
    initial?: Partial<Team> | null
    loading?: boolean
    error?: string
    submitLabel: string
    withCrest?: boolean
  }>(),
  { initial: null, loading: false, error: '', withCrest: false },
)

const emit = defineEmits<{
  (e: 'submit', value: { payload: CreateTeamPayload; crest: File | null }): void
}>()

const { t } = useI18n()

const model = reactive({
  name: '',
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
  colorPrimary: '',
  colorSecondary: '',
  contactEmail: '',
  contactPhone: '',
  website: '',
  crest: null as File | File[] | null,
})

const errors = reactive<Record<string, string>>({})

watch(
  () => props.initial,
  (team) => {
    if (!team) return
    model.name = team.name ?? ''
    model.coach = team.coach ?? ''
    model.city = team.city ?? ''
    model.type = team.type ?? null
    model.division = team.division ?? null
    model.category = team.category ?? null
    model.shortName = team.shortName ?? ''
    model.foundedYear = team.foundedYear ?? null
    model.venueName = team.venueName ?? ''
    model.venueAddress = team.venueAddress ?? ''
    model.venueSurface = team.venueSurface ?? null
    model.venueMapsUrl = team.venueMapsUrl ?? ''
    model.colorPrimary = team.colorPrimary ?? ''
    model.colorSecondary = team.colorSecondary ?? ''
    model.contactEmail = team.contactEmail ?? ''
    model.contactPhone = team.contactPhone ?? ''
    model.website = team.website ?? ''
  },
  { immediate: true },
)

const enumItems = <T extends string>(values: readonly T[], prefix: string) =>
  values.map((value) => ({ value, title: t(`${prefix}.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES, 'profile.team.enums'))
const divisionItems = computed(() => enumItems(DIVISIONS, 'profile.team.enums'))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES, 'profile.team.enums'))
const surfaceItems = computed(() => enumItems(PITCH_SURFACES, 'profile.team.surfaces'))

function crestFile(): File | null {
  const value = model.crest
  if (Array.isArray(value)) return value[0] ?? null
  return value
}

const anyVenueField = computed(
  () =>
    !!model.venueName.trim() ||
    !!model.venueAddress.trim() ||
    !!model.venueSurface ||
    !!model.venueMapsUrl.trim(),
)
const anyColour = computed(() => !!model.colorPrimary.trim() || !!model.colorSecondary.trim())

function isHttpUrl(value: string): boolean {
  try {
    const url = new URL(value.trim())
    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}

function validate(): boolean {
  const required = t('profile.team.required')
  for (const key of Object.keys(errors)) delete errors[key]

  if (!model.name.trim()) errors.name = required
  if (!model.coach.trim()) errors.coach = required
  if (!model.city.trim()) errors.city = required
  if (!model.type) errors.type = required
  if (!model.division) errors.division = required
  if (!model.category) errors.category = required
  if (props.withCrest && !crestFile()) errors.crest = t('profile.team.crestRequired')

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

const optionalOpen = ref(false)

function trimmedOrUndefined(value: string): string | undefined {
  const trimmed = value.trim()
  return trimmed ? trimmed : undefined
}

function submit() {
  if (!validate()) {
    optionalOpen.value = optionalOpen.value || hasOptionalError()
    return
  }

  const payload: CreateTeamPayload = {
    name: model.name.trim(),
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
    contactEmail: trimmedOrUndefined(model.contactEmail),
    contactPhone: trimmedOrUndefined(model.contactPhone),
    website: trimmedOrUndefined(model.website),
  }

  emit('submit', { payload, crest: crestFile() })
}

function hasOptionalError(): boolean {
  return [
    'shortName',
    'foundedYear',
    'venueName',
    'venueAddress',
    'venueSurface',
    'venueMapsUrl',
    'colorPrimary',
    'colorSecondary',
    'contactEmail',
    'contactPhone',
    'website',
  ].some((key) => errors[key])
}
</script>

<template>
  <v-form novalidate @submit.prevent="submit">
    <v-text-field
      v-model="model.name"
      :label="t('profile.team.name')"
      :error-messages="errors.name"
      class="mb-2"
    />
    <v-text-field
      v-model="model.coach"
      :label="t('profile.team.coach')"
      :error-messages="errors.coach"
      class="mb-2"
    />
    <v-text-field
      v-model="model.city"
      :label="t('profile.team.city')"
      :error-messages="errors.city"
      class="mb-2"
    />
    <v-select
      v-model="model.type"
      :items="typeItems"
      variant="outlined"
      density="comfortable"
      :label="t('profile.team.type')"
      :error-messages="errors.type"
      class="mb-2"
    />
    <v-select
      v-model="model.division"
      :items="divisionItems"
      variant="outlined"
      density="comfortable"
      :label="t('profile.team.division')"
      :error-messages="errors.division"
      class="mb-2"
    />
    <v-select
      v-model="model.category"
      :items="categoryItems"
      variant="outlined"
      density="comfortable"
      :label="t('profile.team.category')"
      :error-messages="errors.category"
      class="mb-2"
    />

    <v-file-input
      v-if="withCrest"
      v-model="model.crest"
      variant="outlined"
      density="comfortable"
      accept="image/png,image/jpeg,image/webp,image/svg+xml"
      prepend-icon=""
      :prepend-inner-icon="mdiImageOutline"
      :label="t('profile.team.crest')"
      :error-messages="errors.crest"
      class="mb-2"
    />

    <v-expansion-panels v-model="optionalOpen" flat class="mb-3 team-form-optional">
      <v-expansion-panel :value="true">
        <v-expansion-panel-title>{{ t('profile.team.optionalTitle') }}</v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-text-field
            v-model="model.shortName"
            :label="t('profile.team.shortName')"
            :error-messages="errors.shortName"
            maxlength="20"
            class="mb-2"
          />
          <v-text-field
            v-model.number="model.foundedYear"
            type="number"
            :label="t('profile.team.foundedYear')"
            :error-messages="errors.foundedYear"
            class="mb-2"
          />

          <p class="text-caption text-medium-emphasis mt-2 mb-1">{{ t('profile.team.venueGroup') }}</p>
          <v-text-field
            v-model="model.venueName"
            :label="t('profile.team.venueName')"
            :error-messages="errors.venueName"
            class="mb-2"
          />
          <v-text-field
            v-model="model.venueAddress"
            :label="t('profile.team.venueAddress')"
            :error-messages="errors.venueAddress"
            class="mb-2"
          />
          <v-select
            v-model="model.venueSurface"
            :items="surfaceItems"
            variant="outlined"
            density="comfortable"
            clearable
            :label="t('profile.team.venueSurface')"
            :error-messages="errors.venueSurface"
            class="mb-2"
          />
          <v-text-field
            v-model="model.venueMapsUrl"
            :label="t('profile.team.venueMapsUrl')"
            :hint="t('profile.team.venueMapsUrlHint')"
            persistent-hint
            :error-messages="errors.venueMapsUrl"
            class="mb-3"
          />

          <p class="text-caption text-medium-emphasis mt-2 mb-1">{{ t('profile.team.colorsGroup') }}</p>
          <div class="d-flex flex-column flex-sm-row ga-0 ga-sm-2">
            <v-text-field
              v-model="model.colorPrimary"
              :label="t('profile.team.colorPrimary')"
              :error-messages="errors.colorPrimary"
              class="mb-2"
            />
            <v-text-field
              v-model="model.colorSecondary"
              :label="t('profile.team.colorSecondary')"
              :error-messages="errors.colorSecondary"
              class="mb-2"
            />
          </div>

          <p class="text-caption text-medium-emphasis mt-2 mb-1">{{ t('profile.team.contactGroup') }}</p>
          <v-text-field
            v-model="model.contactEmail"
            type="email"
            :label="t('profile.team.contactEmail')"
            :error-messages="errors.contactEmail"
            class="mb-2"
          />
          <v-text-field
            v-model="model.contactPhone"
            :label="t('profile.team.contactPhone')"
            :error-messages="errors.contactPhone"
            class="mb-2"
          />
          <v-text-field
            v-model="model.website"
            :label="t('profile.team.website')"
            :error-messages="errors.website"
            class="mb-1"
          />
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

    <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-4">
      {{ error }}
    </v-alert>

    <v-btn type="submit" block size="large" variant="tonal" :loading="loading">
      {{ submitLabel }}
    </v-btn>
  </v-form>
</template>

<style scoped>
.team-form-optional :deep(.v-expansion-panel-title) {
  min-height: 44px;
  padding-inline: 0;
}
.team-form-optional :deep(.v-expansion-panel-text__wrapper) {
  padding-inline: 0;
}
</style>
