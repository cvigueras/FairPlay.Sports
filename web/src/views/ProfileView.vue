<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAccountOutline,
  mdiCardAccountDetailsOutline,
  mdiMapMarkerOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiTrophyOutline,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import TeamCrest from '@/components/TeamCrest.vue'
import TeamForm from '@/components/TeamForm.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { useAuthStore } from '@/stores/auth'
import type { CreateTeamPayload, Team } from '@/types/team'

const auth = useAuthStore()
const { t, locale } = useI18n()
const { smAndDown } = useDisplay()

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

const creatingTeam = ref(false)
const createError = ref('')

async function handleCreate({ payload, crest }: { payload: CreateTeamPayload; crest: File | null }) {
  createError.value = ''
  creatingTeam.value = true
  try {
    const created = await teamsApi.create(payload, auth.accessToken)
    if (crest) await teamsApi.uploadCrest(created.id, crest, auth.accessToken)

    const withCrest = { ...created, hasCrest: !!crest }
    teams.value = [...teams.value, withCrest].sort((a, b) => a.name.localeCompare(b.name))
    selectedTeamId.value = created.id
    await auth.setTeam(created.id)
    myTeam.value = withCrest
    teamSaved.value = true
  } catch (error) {
    createError.value = error instanceof ApiError ? error.message : t('profile.team.createFailed')
  } finally {
    creatingTeam.value = false
  }
}

/* ---- Edit my team ------------------------------------------------------------ */

const editOpen = ref(false)
const savingEdit = ref(false)
const editError = ref('')

async function handleUpdate({ payload }: { payload: CreateTeamPayload; crest: File | null }) {
  if (!myTeam.value) return
  editError.value = ''
  savingEdit.value = true
  try {
    const updated = await teamsApi.update(myTeam.value.id, payload, auth.accessToken)
    myTeam.value = updated
    teams.value = teams.value.map((tm) => (tm.id === updated.id ? updated : tm))
    editOpen.value = false
  } catch (error) {
    editError.value = error instanceof ApiError ? error.message : t('profile.team.updateFailed')
  } finally {
    savingEdit.value = false
  }
}
</script>

<template>
  <v-main>
    <v-container v-if="user" class="py-6 py-md-10 profile-container">
      <v-alert
        v-if="!user.teamId"
        type="warning"
        variant="tonal"
        density="comfortable"
        class="mb-6"
      >
        {{ t('profile.activation.needsTeam') }}
      </v-alert>

      <!-- Has a team: the user's profile on top, the team's details below. -->
      <template v-if="user.teamId">
        <v-row>
          <v-col cols="12">
            <v-card
              border
              flat
              rounded="xl"
              class="px-4 py-4 px-sm-8 py-sm-3 d-flex align-center ga-4 ga-sm-6"
            >
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

          <v-col cols="12">
            <v-card
              v-if="myTeam"
              border
              flat
              rounded="xl"
              class="bkt bkt2 px-4 py-3 px-md-6"
            >
              <div class="bkt-crest">
                <TeamCrest :team="myTeam" :size="76" />
              </div>
              <div class="bkt-body">
                <div class="bkt-row1">
                  <span class="text-subtitle-1 font-weight-bold">{{ myTeam.name }}</span>
                  <span class="text-disabled">·</span>
                  <span
                    class="text-subtitle-1 font-weight-bold"
                    :style="{ color: AGE_CATEGORY_COLOR[myTeam.category] }"
                  >
                    {{ t(`profile.team.enums.${myTeam.category}`) }}
                  </span>
                </div>
                <div class="bkt-chips">
                  <v-chip
                    size="x-small"
                    variant="tonal"
                    :color="MODALITY_COLOR[myTeam.type]"
                    :prepend-icon="mdiSoccer"
                  >
                    {{ t(`profile.team.enums.${myTeam.type}`) }}
                  </v-chip>
                  <v-chip
                    size="x-small"
                    variant="tonal"
                    :color="DIVISION_COLOR[myTeam.division]"
                    :prepend-icon="mdiTrophyOutline"
                  >
                    {{ t(`profile.team.enums.${myTeam.division}`) }}
                  </v-chip>
                  <v-chip size="x-small" variant="tonal" :prepend-icon="mdiMapMarkerOutline">
                    {{ myTeam.city }}
                  </v-chip>
                </div>
                <div class="bkt-meta text-body-2 text-medium-emphasis">
                  <span>
                    <v-icon size="14" :icon="mdiAccountOutline" color="#5D4037" />
                    {{ myTeam.coach }}
                  </span>
                  <span v-if="myTeam.venueName">
                    <v-icon size="14" :icon="mdiSoccerField" color="#2E7D32" />
                    {{ myTeam.venueName }}
                  </span>
                </div>
              </div>
              <div class="bkt-actions">
                <!-- Mobile: a bigger, labelled button is an easier tap target. -->
                <v-btn
                  v-if="smAndDown"
                  :to="{ name: 'team-detail', params: { id: myTeam.id } }"
                  :prepend-icon="mdiCardAccountDetailsOutline"
                  color="blue"
                  variant="outlined"
                  block
                >
                  {{ t('profile.team.viewDetails') }}
                </v-btn>
                <v-tooltip v-else :text="t('profile.team.viewDetails')" location="top">
                  <template #activator="{ props: tooltipProps }">
                    <v-btn
                      v-bind="tooltipProps"
                      :to="{ name: 'team-detail', params: { id: myTeam.id } }"
                      :icon="mdiCardAccountDetailsOutline"
                      color="blue"
                      variant="outlined"
                      size="small"
                      :aria-label="t('profile.team.viewDetails')"
                    />
                  </template>
                </v-tooltip>
              </div>
            </v-card>

            <v-card v-else border flat rounded="xl" class="px-4 py-4 px-md-8 py-md-1">
              <v-progress-circular
                indeterminate
                color="primary"
                class="d-block mx-auto my-10"
              />
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

            <TeamForm
              with-crest
              :submit-label="creatingTeam ? t('profile.team.creating') : t('profile.team.create')"
              :loading="creatingTeam"
              :error="createError"
              @submit="handleCreate"
            />
          </v-card>
        </v-col>
      </v-row>
    </v-container>

    <!-- Edit my team -->
    <v-dialog v-model="editOpen" max-width="560" scrollable>
      <v-card v-if="myTeam" border flat rounded="xl" class="pa-6">
        <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.editTitle') }}</h2>
        <TeamForm
          :initial="myTeam"
          :submit-label="t('profile.team.saveChanges')"
          :loading="savingEdit"
          :error="editError"
          @submit="handleUpdate"
        />
      </v-card>
    </v-dialog>
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

/* Club panel - shared visual language with the "Teams" list. */
.bkt {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  overflow: hidden;
}
.bkt-crest {
  flex-shrink: 0;
}
.bkt2 .bkt-crest {
  margin-right: 0.75rem;
}
/* Slightly larger chip text without growing the fixed x-small chip height. */
.bkt2 .bkt-chips :deep(.v-chip) {
  font-size: 0.75rem;
}
.bkt-body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.bkt-row1 {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.bkt-chips {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
}
.bkt-meta {
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
}
.bkt-meta span {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  min-width: 0;
}
.bkt-actions {
  flex-shrink: 0;
  align-self: center;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* Phones: let the action drop below the body as a full-width, centred row. */
@media (max-width: 599px) {
  .bkt {
    flex-wrap: wrap;
  }
  .bkt-actions {
    width: 100%;
    justify-content: center;
  }
}
</style>
