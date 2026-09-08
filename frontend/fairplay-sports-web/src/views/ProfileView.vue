<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { mdiCircle, mdiImageOutline, mdiLogout, mdiTranslate } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import { useAuthStore } from '@/stores/auth'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  type AgeCategory,
  type Division,
  type FootballType,
  type Team,
} from '@/types/team'

const router = useRouter()
const auth = useAuthStore()
const { t, locale } = useI18n()

const user = computed(() => auth.currentUser)
const isLoggingOut = ref(false)

const initials = computed(() =>
  (user.value?.userName ?? '')
    .split(/[\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join(''),
)

const memberSince = computed(() => {
  if (!user.value) return ''
  return new Date(user.value.createdAt).toLocaleDateString(locale.value, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

const details = computed(() => {
  if (!user.value) return []
  return [
    { label: t('profile.fields.userName'), value: user.value.userName },
    { label: t('profile.fields.email'), value: user.value.email },
    { label: t('profile.fields.role'), value: user.value.role },
    { label: t('profile.fields.memberSince'), value: memberSince.value },
    {
      label: t('profile.fields.status'),
      value: user.value.active ? t('profile.status.active') : t('profile.status.inactive'),
    },
  ]
})

async function handleLogout() {
  isLoggingOut.value = true
  try {
    await auth.logout()
    await router.push('/login')
  } finally {
    isLoggingOut.value = false
  }
}

/* ---- My team ---------------------------------------------------------------- */

const teams = ref<Team[]>([])
const loadingTeams = ref(false)
const selectedTeamId = ref<string | null>(null)
const savingTeam = ref(false)
const teamSaved = ref(false)
const teamError = ref('')

const selectedTeam = computed(() => teams.value.find((team) => team.id === selectedTeamId.value) ?? null)

const enumItems = <T extends string>(values: readonly T[]) =>
  values.map((value) => ({ value, title: t(`profile.team.enums.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES))
const divisionItems = computed(() => enumItems(DIVISIONS))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES))

onMounted(async () => {
  selectedTeamId.value = user.value?.teamId ?? null
  loadingTeams.value = true
  try {
    teams.value = await teamsApi.list(auth.accessToken)
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.loadFailed')
  } finally {
    loadingTeams.value = false
  }
})

async function saveTeam() {
  if (!selectedTeamId.value) return
  teamError.value = ''
  teamSaved.value = false
  savingTeam.value = true
  try {
    await auth.setTeam(selectedTeamId.value)
    teamSaved.value = true
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.saveFailed')
  } finally {
    savingTeam.value = false
  }
}

/* ---- Create a new team ---------------------------------------------------- */

const newTeam = reactive({
  name: '',
  coach: '',
  city: '',
  type: null as FootballType | null,
  division: null as Division | null,
  category: null as AgeCategory | null,
  crest: null as File | File[] | null,
})

const newTeamErrors = reactive({
  name: '',
  coach: '',
  city: '',
  type: '',
  division: '',
  category: '',
  crest: '',
})

const creatingTeam = ref(false)
const createError = ref('')

function crestFile(): File | null {
  const value = newTeam.crest
  if (Array.isArray(value)) return value[0] ?? null
  return value
}

function validateNewTeam(): boolean {
  newTeamErrors.name = newTeam.name.trim() ? '' : t('profile.team.required')
  newTeamErrors.coach = newTeam.coach.trim() ? '' : t('profile.team.required')
  newTeamErrors.city = newTeam.city.trim() ? '' : t('profile.team.required')
  newTeamErrors.type = newTeam.type ? '' : t('profile.team.required')
  newTeamErrors.division = newTeam.division ? '' : t('profile.team.required')
  newTeamErrors.category = newTeam.category ? '' : t('profile.team.required')
  newTeamErrors.crest = crestFile() ? '' : t('profile.team.crestRequired')

  return !Object.values(newTeamErrors).some(Boolean)
}

async function createTeam() {
  createError.value = ''
  if (!validateNewTeam()) return

  const file = crestFile()!
  creatingTeam.value = true
  try {
    const created = await teamsApi.create(
      {
        name: newTeam.name.trim(),
        coach: newTeam.coach.trim(),
        city: newTeam.city.trim(),
        type: newTeam.type!,
        division: newTeam.division!,
        category: newTeam.category!,
      },
      auth.accessToken,
    )
    await teamsApi.uploadCrest(created.id, file, auth.accessToken)

    teams.value = [...teams.value, { ...created, hasCrest: true }].sort((a, b) =>
      a.name.localeCompare(b.name),
    )
    selectedTeamId.value = created.id
    await auth.setTeam(created.id)
    teamSaved.value = true

    newTeam.name = ''
    newTeam.coach = ''
    newTeam.city = ''
    newTeam.type = null
    newTeam.division = null
    newTeam.category = null
    newTeam.crest = null
  } catch (error) {
    createError.value = error instanceof ApiError ? error.message : t('profile.team.createFailed')
  } finally {
    creatingTeam.value = false
  }
}
</script>

<template>
  <v-app-bar flat border="b" color="surface">
    <v-app-bar-title>
      <span class="d-inline-flex align-center ga-2 font-weight-bold">
        <v-icon :icon="mdiCircle" color="primary" size="12" />
        {{ t('common.appName') }}
      </span>
    </v-app-bar-title>

    <template #append>
      <v-menu>
        <template #activator="{ props }">
          <v-btn
            variant="text"
            :prepend-icon="mdiTranslate"
            :aria-label="t('language.label')"
            v-bind="props"
          >
            {{ locale.toUpperCase() }}
          </v-btn>
        </template>
        <v-list density="compact">
          <v-list-item
            v-for="code in SUPPORTED_LOCALES"
            :key="code"
            :active="code === locale"
            @click="setLocale(code)"
          >
            <v-list-item-title>{{ t(`language.${code}`) }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>

      <span v-if="user" class="text-body-2 text-medium-emphasis mx-3 d-none d-sm-inline">
        {{ user.userName }}
      </span>

      <v-btn
        variant="outlined"
        :prepend-icon="mdiLogout"
        :loading="isLoggingOut"
        @click="handleLogout"
      >
        {{ isLoggingOut ? t('profile.loggingOut') : t('profile.logout') }}
      </v-btn>
    </template>
  </v-app-bar>

  <v-main>
    <v-container v-if="user" class="py-10 profile-container">
      <v-alert
        v-if="!user.teamId"
        type="warning"
        variant="tonal"
        density="comfortable"
        class="mb-6"
      >
        {{ t('profile.activation.needsTeam') }}
      </v-alert>

      <v-row>
        <!-- Top left: profile summary -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6 d-flex align-center ga-4 h-100">
            <v-avatar color="primary" size="64" class="text-h6 font-weight-bold">
              {{ initials }}
            </v-avatar>
            <div class="flex-grow-1 overflow-hidden">
              <p class="text-h6 font-weight-bold text-truncate">{{ user.userName }}</p>
              <p class="text-body-2 text-medium-emphasis text-truncate">{{ user.email }}</p>
              <v-chip
                :color="user.role === 'Admin' ? 'amber-darken-2' : 'primary'"
                size="small"
                variant="tonal"
                class="mt-1"
              >
                {{ user.role }}
              </v-chip>
            </div>
          </v-card>
        </v-col>

        <!-- Top right: the team the user belongs to -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6 h-100">
            <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.title') }}</h2>

            <v-select
              v-model="selectedTeamId"
              :items="teams"
              item-title="name"
              item-value="id"
              variant="outlined"
              density="comfortable"
              :label="t('profile.team.select')"
              :placeholder="t('profile.team.selectPlaceholder')"
              persistent-placeholder
              :loading="loadingTeams"
              hide-details="auto"
              class="mb-4"
            />

            <div v-if="selectedTeam" class="mb-4 text-center">
              <v-img
                v-if="selectedTeam.hasCrest"
                :src="teamsApi.crestUrl(selectedTeam.id)"
                :alt="selectedTeam.name"
                height="160"
                class="mx-auto"
                style="max-width: 200px"
              />
              <p v-else class="text-caption text-medium-emphasis py-4">
                {{ t('profile.team.noCrest') }}
              </p>
            </div>

            <v-alert
              v-if="teamError"
              type="error"
              variant="tonal"
              density="compact"
              class="mb-4"
            >
              {{ teamError }}
            </v-alert>
            <v-alert
              v-else-if="teamSaved"
              type="success"
              variant="tonal"
              density="compact"
              class="mb-4"
            >
              {{ t('profile.team.saved') }}
            </v-alert>

            <v-btn
              block
              size="large"
              :loading="savingTeam"
              :disabled="!selectedTeamId || selectedTeamId === user.teamId"
              @click="saveTeam"
            >
              {{ t('profile.team.save') }}
            </v-btn>
          </v-card>
        </v-col>
      </v-row>

      <v-row class="mt-6">
        <!-- Bottom left: profile details -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl">
            <v-list>
              <template v-for="(row, index) in details" :key="row.label">
                <v-divider v-if="index > 0" />
                <v-list-item class="py-3">
                  <template #subtitle>
                    <span class="text-caption text-uppercase">{{ row.label }}</span>
                  </template>
                  <v-list-item-title class="font-weight-medium">{{ row.value }}</v-list-item-title>
                </v-list-item>
              </template>
            </v-list>
          </v-card>
        </v-col>

        <!-- Bottom right: create a new team -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6">
            <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.createTitle') }}</h2>

            <v-form novalidate @submit.prevent="createTeam">
              <v-text-field
                v-model="newTeam.name"
                :label="t('profile.team.name')"
                :error-messages="newTeamErrors.name"
                class="mb-2"
              />
              <v-text-field
                v-model="newTeam.coach"
                :label="t('profile.team.coach')"
                :error-messages="newTeamErrors.coach"
                class="mb-2"
              />
              <v-text-field
                v-model="newTeam.city"
                :label="t('profile.team.city')"
                :error-messages="newTeamErrors.city"
                class="mb-2"
              />
              <v-select
                v-model="newTeam.type"
                :items="typeItems"
                variant="outlined"
                density="comfortable"
                :label="t('profile.team.type')"
                :error-messages="newTeamErrors.type"
                class="mb-2"
              />
              <v-select
                v-model="newTeam.division"
                :items="divisionItems"
                variant="outlined"
                density="comfortable"
                :label="t('profile.team.division')"
                :error-messages="newTeamErrors.division"
                class="mb-2"
              />
              <v-select
                v-model="newTeam.category"
                :items="categoryItems"
                variant="outlined"
                density="comfortable"
                :label="t('profile.team.category')"
                :error-messages="newTeamErrors.category"
                class="mb-2"
              />
              <v-file-input
                v-model="newTeam.crest"
                variant="outlined"
                density="comfortable"
                accept="image/png,image/jpeg,image/webp,image/svg+xml"
                prepend-icon=""
                :prepend-inner-icon="mdiImageOutline"
                :label="t('profile.team.crest')"
                :error-messages="newTeamErrors.crest"
                class="mb-2"
              />

              <v-alert
                v-if="createError"
                type="error"
                variant="tonal"
                density="compact"
                class="mb-4"
              >
                {{ createError }}
              </v-alert>

              <v-btn type="submit" block size="large" variant="tonal" :loading="creatingTeam">
                {{ creatingTeam ? t('profile.team.creating') : t('profile.team.create') }}
              </v-btn>
            </v-form>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-main>
</template>
