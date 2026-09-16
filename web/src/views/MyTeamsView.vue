<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiAccountOutline,
  mdiAccountRemoveOutline,
  mdiCardAccountDetailsOutline,
  mdiMapMarkerOutline,
  mdiPencilOutline,
  mdiPlusCircleOutline,
  mdiSoccer,
  mdiSoccerField,
  mdiTrophyOutline,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import TeamCrest from '@/components/TeamCrest.vue'
import TeamForm from '@/components/TeamForm.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { useAuthStore } from '@/stores/auth'
import type { CreateTeamPayload, Team } from '@/types/team'
import { TEAM_MEMBER_ROLES, type TeamMemberRole, type TeamMembership } from '@/types/team'

const auth = useAuthStore()
const { t } = useI18n()

const user = computed(() => auth.currentUser)
const myTeams = computed(() => auth.myTeams)

const memberRoleItems = computed(() =>
  TEAM_MEMBER_ROLES.map((role) => ({ value: role, title: t(`profile.team.memberRoles.${role}`) })),
)

/* ---- My teams ----------------------------------------------------------
   A user can belong to several teams. `myTeams` (the memberships) comes from
   the auth store; the actual Team records are fetched by id here (fetched
   individually rather than found in `teams` below, which is capped at the
   backend's max page size). */

const teams = ref<Team[]>([])
const loadingTeams = ref(false)
const teamDetails = ref<Record<string, Team>>({})
const loadingMyTeams = ref(false)
const selectedTeamId = ref<string | null>(null)
const joinRole = ref<TeamMemberRole | null>(null)
const joinDisplayName = ref('')
const joinValid = computed(() => !!joinRole.value && joinDisplayName.value.trim().length > 0)
const savingTeam = ref(false)
const teamSaved = ref(false)
const teamError = ref('')

const joinableTeams = computed(() =>
  teams.value.filter((team) => !myTeams.value.some((membership) => membership.teamId === team.id)),
)
const selectedTeam = computed(() => teams.value.find((team) => team.id === selectedTeamId.value) ?? null)

/** Each membership paired with its resolved `Team`, once fetched. */
const myTeamCards = computed(() =>
  myTeams.value
    .map((membership) => ({ membership, team: teamDetails.value[membership.teamId] }))
    .filter((card): card is { membership: TeamMembership; team: Team } => !!card.team),
)

/** Shown when there's nothing to show yet, or the user asks to join/found another team. */
const showJoinPanel = ref(true)

async function loadMyTeamDetails() {
  loadingMyTeams.value = true
  try {
    const details = await Promise.all(
      myTeams.value.map((membership) => teamsApi.byId(membership.teamId, auth.accessToken)),
    )
    teamDetails.value = Object.fromEntries(details.map((team) => [team.id, team]))
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.loadFailed')
  } finally {
    loadingMyTeams.value = false
  }
}

onMounted(async () => {
  loadingTeams.value = true
  try {
    const [list] = await Promise.all([teamsApi.list(auth.accessToken), auth.loadMyTeams()])
    teams.value = list
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.loadFailed')
  } finally {
    loadingTeams.value = false
  }

  await loadMyTeamDetails()
  showJoinPanel.value = myTeams.value.length === 0
})

async function saveTeam() {
  if (!selectedTeamId.value || !joinRole.value) return
  teamError.value = ''
  teamSaved.value = false
  savingTeam.value = true
  try {
    await auth.joinTeam(selectedTeamId.value, joinRole.value, joinDisplayName.value.trim())
    await loadMyTeamDetails()
    teamSaved.value = true
    showJoinPanel.value = false
    selectedTeamId.value = null
    joinRole.value = null
    joinDisplayName.value = ''
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.saveFailed')
  } finally {
    savingTeam.value = false
  }
}

/* ---- Create a new team ---------------------------------------------------- */

const createRole = ref<TeamMemberRole | null>(null)
const createDisplayName = ref('')
const createValid = computed(() => !!createRole.value && createDisplayName.value.trim().length > 0)
const creatingTeam = ref(false)
const createError = ref('')

async function handleCreate({ payload, crest }: { payload: CreateTeamPayload; crest: File | null }) {
  if (!createRole.value) return
  createError.value = ''
  creatingTeam.value = true
  try {
    const created = await teamsApi.create(payload, auth.accessToken)
    if (crest) await teamsApi.uploadCrest(created.id, crest, auth.accessToken)

    const withCrest = { ...created, hasCrest: !!crest }
    teams.value = [...teams.value, withCrest].sort((a, b) => a.name.localeCompare(b.name))
    await auth.joinTeam(created.id, createRole.value, createDisplayName.value.trim())
    teamDetails.value = { ...teamDetails.value, [created.id]: withCrest }
    teamSaved.value = true
    showJoinPanel.value = false
    createRole.value = null
    createDisplayName.value = ''
  } catch (error) {
    createError.value = error instanceof ApiError ? error.message : t('profile.team.createFailed')
  } finally {
    creatingTeam.value = false
  }
}

/* ---- Edit a team ------------------------------------------------------------ */

const editingTeamId = ref<string | null>(null)
const editingTeam = computed(() =>
  editingTeamId.value ? (teamDetails.value[editingTeamId.value] ?? null) : null,
)
const savingEdit = ref(false)
const editError = ref('')

function openEdit(teamId: string) {
  editingTeamId.value = teamId
}

async function handleUpdate({ payload }: { payload: CreateTeamPayload; crest: File | null }) {
  if (!editingTeam.value) return
  editError.value = ''
  savingEdit.value = true
  try {
    const updated = await teamsApi.update(editingTeam.value.id, payload, auth.accessToken)
    teamDetails.value = { ...teamDetails.value, [updated.id]: updated }
    teams.value = teams.value.map((tm) => (tm.id === updated.id ? updated : tm))
    editingTeamId.value = null
  } catch (error) {
    editError.value = error instanceof ApiError ? error.message : t('profile.team.updateFailed')
  } finally {
    savingEdit.value = false
  }
}

/* ---- Leave a team ------------------------------------------------------------ */

const leavingTeamId = ref<string | null>(null)
const leaveError = ref('')

async function confirmLeave() {
  if (!leavingTeamId.value) return
  const teamId = leavingTeamId.value
  leaveError.value = ''
  try {
    await auth.leaveTeam(teamId)
    const { [teamId]: _removed, ...rest } = teamDetails.value
    teamDetails.value = rest
    leavingTeamId.value = null
  } catch (error) {
    leaveError.value = error instanceof ApiError ? error.message : t('profile.team.leaveFailed')
  }
}
</script>

<template>
  <v-main>
    <v-container v-if="user" class="py-6 py-md-10 profile-container">
      <!-- The page title moved to the breadcrumb (see AppShell). -->
      <v-alert
        v-if="myTeams.length === 0"
        type="warning"
        variant="tonal"
        density="comfortable"
        class="mb-5"
      >
        {{ t('profile.activation.needsTeam') }}
      </v-alert>

      <!-- Has teams: show one card per membership. -->
      <v-row v-if="myTeams.length > 0" class="mb-4">
        <v-col v-for="{ membership, team } in myTeamCards" :key="membership.id" cols="12">
          <v-card border flat rounded="xl" class="bkt bkt2 px-4 py-3 px-md-6">
            <div class="bkt-crest">
              <TeamCrest :team="team" :size="76" />
            </div>
            <div class="bkt-body">
              <div class="bkt-row1">
                <span class="text-h5 font-weight-bold">{{ team.name }}</span>
                <v-chip size="default" variant="tonal" color="primary" class="text-subtitle-1 font-weight-medium">
                  {{ t(`profile.team.memberRoles.${membership.role}`) }}
                </v-chip>
              </div>
              <div class="bkt-chips">
                <v-chip
                  size="x-small"
                  variant="tonal"
                  :color="MODALITY_COLOR[team.type]"
                  :prepend-icon="mdiSoccer"
                >
                  {{ t(`profile.team.enums.${team.type}`) }}
                </v-chip>
                <v-chip
                  size="x-small"
                  variant="tonal"
                  :color="DIVISION_COLOR[team.division]"
                  :prepend-icon="mdiTrophyOutline"
                >
                  {{ t(`profile.team.enums.${team.division}`) }}
                </v-chip>
                <v-chip size="x-small" variant="tonal" :color="AGE_CATEGORY_COLOR[team.category]">
                  {{ t(`profile.team.enums.${team.category}`) }}
                </v-chip>
                <v-chip size="x-small" variant="tonal" :prepend-icon="mdiMapMarkerOutline">
                  {{ team.city }}
                </v-chip>
              </div>
              <div class="bkt-meta text-body-2 text-medium-emphasis">
                <span>
                  <v-icon size="14" :icon="mdiAccountOutline" color="#5D4037" />
                  {{ membership.displayName }}
                </span>
                <span v-if="team.venueName">
                  <v-icon size="14" :icon="mdiSoccerField" color="#2E7D32" />
                  {{ team.venueName }}
                </span>
              </div>
            </div>
            <div class="bkt-actions">
              <v-tooltip :text="t('profile.team.viewDetails')" location="top">
                <template #activator="{ props: tooltipProps }">
                  <v-btn
                    v-bind="tooltipProps"
                    :to="{ name: 'team-detail', params: { id: team.id } }"
                    :icon="mdiCardAccountDetailsOutline"
                    color="blue"
                    variant="outlined"
                    size="small"
                    :aria-label="t('profile.team.viewDetails')"
                    @click.stop
                  />
                </template>
              </v-tooltip>
              <v-tooltip :text="t('profile.team.editTitle')" location="top">
                <template #activator="{ props: tooltipProps }">
                  <v-btn
                    v-bind="tooltipProps"
                    :icon="mdiPencilOutline"
                    color="blue"
                    variant="outlined"
                    size="small"
                    :aria-label="t('profile.team.editTitle')"
                    @click="openEdit(team.id)"
                  />
                </template>
              </v-tooltip>
              <v-tooltip :text="t('profile.team.leave')" location="top">
                <template #activator="{ props: tooltipProps }">
                  <v-btn
                    v-bind="tooltipProps"
                    :icon="mdiAccountRemoveOutline"
                    color="error"
                    variant="outlined"
                    size="small"
                    :aria-label="t('profile.team.leave')"
                    @click="leavingTeamId = team.id"
                  />
                </template>
              </v-tooltip>
            </div>
          </v-card>
        </v-col>
      </v-row>

      <v-btn
        v-if="myTeams.length > 0 && !showJoinPanel"
        variant="tonal"
        :prepend-icon="mdiPlusCircleOutline"
        class="mb-4"
        @click="showJoinPanel = true"
      >
        {{ t('profile.team.joinAnother') }}
      </v-btn>

      <!-- Join or found a team: each card asks who you are there - your role
           and your name/nickname - since that's specific to that team. -->
      <v-row v-if="showJoinPanel">
        <!-- Join an existing team -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6 h-100">
            <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.joinTitle') }}</h2>

            <v-select
              v-model="selectedTeamId"
              :items="joinableTeams"
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

            <v-row dense class="mb-2">
              <v-col cols="12" sm="6">
                <v-select
                  v-model="joinRole"
                  :items="memberRoleItems"
                  variant="outlined"
                  density="comfortable"
                  :label="t('profile.team.memberRole')"
                  hide-details
                />
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="joinDisplayName"
                  variant="outlined"
                  density="comfortable"
                  :label="t('profile.team.displayName')"
                  hide-details
                />
              </v-col>
            </v-row>

            <v-alert
              v-if="teamError"
              type="error"
              variant="tonal"
              density="compact"
              class="my-4"
            >
              {{ teamError }}
            </v-alert>
            <v-alert
              v-else-if="teamSaved"
              type="success"
              variant="tonal"
              density="compact"
              class="my-4"
            >
              {{ t('profile.team.saved') }}
            </v-alert>

            <v-btn
              block
              size="large"
              :loading="savingTeam"
              :disabled="!selectedTeamId || !joinValid"
              @click="saveTeam"
            >
              {{ t('profile.team.save') }}
            </v-btn>
          </v-card>
        </v-col>

        <!-- Create a new team and assign it to yourself -->
        <v-col cols="12" md="6">
          <v-card border flat rounded="xl" class="pa-6 h-100">
            <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.createTitle') }}</h2>

            <v-row dense class="mb-2">
              <v-col cols="12" sm="6">
                <v-select
                  v-model="createRole"
                  :items="memberRoleItems"
                  variant="outlined"
                  density="comfortable"
                  :label="t('profile.team.memberRole')"
                  hide-details
                />
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="createDisplayName"
                  variant="outlined"
                  density="comfortable"
                  :label="t('profile.team.displayName')"
                  hide-details
                />
              </v-col>
            </v-row>

            <TeamForm
              with-crest
              :submit-label="creatingTeam ? t('profile.team.creating') : t('profile.team.create')"
              :loading="creatingTeam"
              :disabled="!createValid"
              :error="createError"
              @submit="handleCreate"
            />
          </v-card>
        </v-col>
      </v-row>
    </v-container>

    <!-- Edit a team -->
    <v-dialog :model-value="!!editingTeam" max-width="560" scrollable @update:model-value="editingTeamId = null">
      <v-card v-if="editingTeam" border flat rounded="xl" class="pa-6">
        <h2 class="text-h6 font-weight-bold mb-4">{{ t('profile.team.editTitle') }}</h2>
        <TeamForm
          :initial="editingTeam"
          :submit-label="t('profile.team.saveChanges')"
          :loading="savingEdit"
          :error="editError"
          @submit="handleUpdate"
        />
      </v-card>
    </v-dialog>

    <!-- Confirm leaving a team -->
    <v-dialog :model-value="!!leavingTeamId" max-width="420" @update:model-value="leavingTeamId = null">
      <v-card border flat rounded="xl" class="pa-6">
        <h2 class="text-h6 font-weight-bold mb-2">{{ t('profile.team.leaveConfirmTitle') }}</h2>
        <p class="text-body-2 text-medium-emphasis mb-4">{{ t('profile.team.leaveConfirmHint') }}</p>
        <v-alert v-if="leaveError" type="error" variant="tonal" density="compact" class="mb-4">
          {{ leaveError }}
        </v-alert>
        <div class="d-flex ga-2 justify-end">
          <v-btn variant="text" @click="leavingTeamId = null">{{ t('profile.team.cancel') }}</v-btn>
          <v-btn color="error" variant="flat" @click="confirmLeave">{{ t('profile.team.leave') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>
  </v-main>
</template>

<style scoped>
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
  margin-bottom: 0.5rem;
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
  margin-top: 0.5rem;
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
  flex-direction: row;
  gap: 0.5rem;
}

/* Phones: let the actions drop below the body as a full-width, centred row. */
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
