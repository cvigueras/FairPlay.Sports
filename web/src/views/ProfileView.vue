<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiImageOutline, mdiShieldOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import ModalityIcon from '@/components/ModalityIcon.vue'
import { useAuthStore } from '@/stores/auth'
import {
  AGE_CATEGORIES,
  DIVISIONS,
  FOOTBALL_TYPES,
  type AgeCategory,
  type Division,
  type FootballType,
  type Team,
} from '@/types/team'

const auth = useAuthStore()
const { t, locale } = useI18n()

const user = computed(() => auth.currentUser)

const formatLongDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'long', day: 'numeric' })

const memberSince = computed(() => (user.value ? formatLongDate(user.value.createdAt) : ''))

/* ---- My team ---------------------------------------------------------------- */

const teams = ref<Team[]>([])
const loadingTeams = ref(false)
const selectedTeamId = ref<string | null>(null)
const savingTeam = ref(false)
const teamSaved = ref(false)
const teamError = ref('')

const selectedTeam = computed(() => teams.value.find((team) => team.id === selectedTeamId.value) ?? null)

/**
 * The team the user currently belongs to. Fetched by id rather than looked up
 * in `teams`, which is capped at the backend's max page size.
 */
const myTeam = ref<Team | null>(null)

const teamDetails = computed(() => {
  const team = myTeam.value
  if (!team) return []
  return [
    { label: t('profile.team.coach'), value: team.coach },
    { label: t('profile.team.city'), value: team.city },
    {
      label: t('profile.team.type'),
      value: t(`profile.team.enums.${team.type}`),
      modality: team.type,
    },
    { label: t('profile.team.division'), value: t(`profile.team.enums.${team.division}`) },
    { label: t('profile.team.category'), value: t(`profile.team.enums.${team.category}`) },
  ]
})

const enumItems = <T extends string>(values: readonly T[]) =>
  values.map((value) => ({ value, title: t(`profile.team.enums.${value}`) }))
const typeItems = computed(() => enumItems(FOOTBALL_TYPES))
const divisionItems = computed(() => enumItems(DIVISIONS))
const categoryItems = computed(() => enumItems(AGE_CATEGORIES))

onMounted(async () => {
  selectedTeamId.value = user.value?.teamId ?? null
  loadingTeams.value = true
  try {
    const [list, mine] = await Promise.all([
      teamsApi.list(auth.accessToken),
      user.value?.teamId
        ? teamsApi.byId(user.value.teamId, auth.accessToken)
        : Promise.resolve(null),
    ])
    teams.value = list
    myTeam.value = mine
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
    myTeam.value = selectedTeam.value ?? myTeam.value
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

      <!-- Has a team: profile on the left, the team's details on the right -->
      <template v-if="user.teamId">
        <v-row>
          <v-col cols="12" md="6">
            <v-card border flat rounded="xl" class="pa-6 d-flex align-center ga-6 h-100">
              <div class="d-flex flex-column align-center flex-shrink-0 ga-4">
                <ProfileAvatar />
                <div class="member-since">
                  <div class="member-since-label text-medium-emphasis">
                    {{ t('profile.fields.memberSince') }}
                  </div>
                  <div class="member-since-bar"></div>
                  <div class="member-since-value font-weight-medium">{{ memberSince }}</div>
                </div>
              </div>
              <div class="flex-grow-1 overflow-hidden ms-6">
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

          <v-col cols="12" md="6">
            <v-card
              v-if="myTeam"
              border
              flat
              rounded="xl"
              class="pa-6 d-flex ga-6 h-100 team-panel"
            >
              <div
                class="d-flex flex-column align-center justify-center flex-shrink-0 ga-4 team-identity"
              >
                <div class="team-name-row">
                  <ModalityIcon :type="myTeam.type" :size="60" />
                  <p class="text-h6 font-weight-bold team-name mb-0">{{ myTeam.name }}</p>
                </div>
                <v-avatar size="120" rounded="lg">
                  <v-img
                    v-if="myTeam.hasCrest"
                    :src="teamsApi.crestUrl(myTeam.id)"
                    :alt="myTeam.name"
                  />
                  <v-icon v-else :icon="mdiShieldOutline" size="56" class="text-medium-emphasis" />
                </v-avatar>
                <div class="member-since">
                  <div class="member-since-label text-medium-emphasis">
                    {{ t('profile.fields.memberSince') }}
                  </div>
                  <div class="member-since-bar"></div>
                  <div class="member-since-value font-weight-medium">
                    {{ formatLongDate(myTeam.createdAt) }}
                  </div>
                </div>
              </div>

              <v-divider vertical class="d-none d-sm-block" />

              <div class="flex-grow-1 team-detail-grid">
                <div
                  v-for="row in teamDetails"
                  :key="row.label"
                  class="team-detail-cell"
                >
                  <div class="text-caption text-medium-emphasis">{{ row.label }}</div>
                  <div class="text-body-1 font-weight-medium mt-1 d-flex align-center ga-2 detail-value">
                    <ModalityIcon v-if="row.modality" :type="row.modality" :size="18" />
                    <span>{{ row.value }}</span>
                  </div>
                </div>
              </div>
            </v-card>

            <v-card v-else border flat rounded="xl" class="pa-6 h-100">
              <v-progress-circular
                indeterminate
                color="primary"
                class="d-block mx-auto my-10"
              />
            </v-card>
          </v-col>
        </v-row>

        <!-- TEMP: preview of every modality icon -->
        <v-row>
          <v-col cols="12">
            <v-card border flat rounded="xl" class="pa-6 d-flex flex-wrap justify-center ga-10">
              <div
                v-for="type in FOOTBALL_TYPES"
                :key="type"
                class="d-flex flex-column align-center ga-2"
              >
                <ModalityIcon :type="type" :size="56" />
                <span class="text-caption text-medium-emphasis">
                  {{ t(`profile.team.enums.${type}`) }}
                </span>
              </div>
            </v-card>
          </v-col>
        </v-row>
      </template>

      <v-row v-if="!user.teamId">
        <!-- Top left: profile summary -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6 d-flex align-center ga-6 h-100">
            <div class="d-flex flex-column align-center flex-shrink-0 ga-4">
              <ProfileAvatar />
              <div class="member-since">
                <div class="member-since-label text-medium-emphasis">
                  {{ t('profile.fields.memberSince') }}
                </div>
                <div class="member-since-bar"></div>
                <div class="member-since-value font-weight-medium">{{ memberSince }}</div>
              </div>
            </div>
            <div class="flex-grow-1 overflow-hidden ms-6">
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

      <v-row v-if="!user.teamId" class="mt-6">
        <!-- Create a new team -->
        <v-col cols="12" md="6" offset-md="6">
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

<style scoped>
/* "Member since" block under the avatar, mirroring the Teams list card. */
.member-since {
  min-width: 0;
  text-align: center;
}

.member-since-label {
  font-size: 0.68rem;
  line-height: 1.2;
}

.member-since-bar {
  width: 100%;
  height: 3px;
  margin: 4px 0;
  border-radius: 2px;
  background: #86efac;
}

.member-since-value {
  font-size: 0.75rem;
  line-height: 1.25;
}

/* Team panel: name + crest + "member since" on the left, boxed details on the right. */
.team-identity {
  width: 176px;
}

.team-name {
  line-height: 1.25;
  overflow-wrap: anywhere;
}

/* Keep the club name optically centred against the modality icon. */
.team-name-row {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  gap: 0.5rem;
}

/* Line the club name up with the top of the modality icon. */
.team-name-row .team-name {
  line-height: 1;
  padding-top: 2px;
}

.detail-value span {
  white-space: nowrap;
}

.team-detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
  align-content: center;
}

/* The first field (coach) runs the full width, the rest form a tidy 2x2. */
.team-detail-cell:first-child {
  grid-column: 1 / -1;
}

.team-detail-cell {
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 12px;
  padding: 0.65rem 0.9rem;
}

@media (max-width: 599px) {
  .team-panel {
    flex-direction: column;
    align-items: stretch;
  }

  .team-identity {
    width: 100%;
  }

  .team-detail-grid {
    grid-template-columns: 1fr;
  }

  .team-detail-cell:first-child {
    grid-column: auto;
  }
}
</style>
