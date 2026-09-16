<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiAccountOutline,
  mdiAccountRemoveOutline,
  mdiCardAccountDetailsOutline,
  mdiPencilOutline,
  mdiPlusCircleOutline,
  mdiShieldOutline,
  mdiSoccerField,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import FlatField from '@/components/FlatField.vue'
import RolePills from '@/components/RolePills.vue'
import TeamForm from '@/components/TeamForm.vue'
import TeamWizard from '@/components/TeamWizard.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { tonalStyle } from '@/lib/tonalColor'
import { useAuthStore } from '@/stores/auth'
import type { CreateTeamPayload, Team, TeamMemberRole, TeamMembership } from '@/types/team'

const auth = useAuthStore()
const { t } = useI18n()

const user = computed(() => auth.currentUser)
const myTeams = computed(() => auth.myTeams)

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
const savingTeam = ref(false)
const teamSaved = ref(false)
const teamError = ref('')
const joinErrors = reactive<Record<string, string>>({})

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
})

function validateJoin(): boolean {
  const required = t('profile.team.required')
  for (const key of Object.keys(joinErrors)) delete joinErrors[key]

  if (!selectedTeamId.value) joinErrors.team = required
  if (!joinRole.value) joinErrors.role = required
  if (!joinDisplayName.value.trim()) joinErrors.displayName = required

  return Object.keys(joinErrors).length === 0
}

async function saveTeam() {
  if (!validateJoin() || !selectedTeamId.value || !joinRole.value) return
  teamError.value = ''
  teamSaved.value = false
  savingTeam.value = true
  try {
    await auth.joinTeam(selectedTeamId.value, joinRole.value, joinDisplayName.value.trim())
    await loadMyTeamDetails()
    teamSaved.value = true
    selectedTeamId.value = null
    joinRole.value = null
    joinDisplayName.value = ''
  } catch (error) {
    teamError.value = error instanceof ApiError ? error.message : t('profile.team.saveFailed')
  } finally {
    savingTeam.value = false
  }
}

/* ---- Create a new team (wizard) --------------------------------------- */

const wizardOpen = ref(false)
const creatingTeam = ref(false)
const createError = ref('')

async function handleCreate({
  payload,
  role,
  displayName,
  crest,
}: {
  payload: CreateTeamPayload
  role: TeamMemberRole
  displayName: string
  crest: File | null
}) {
  createError.value = ''
  creatingTeam.value = true
  try {
    const created = await teamsApi.create(payload, auth.accessToken)
    if (crest) await teamsApi.uploadCrest(created.id, crest, auth.accessToken)

    const withCrest = { ...created, hasCrest: !!crest }
    teams.value = [...teams.value, withCrest].sort((a, b) => a.name.localeCompare(b.name))
    await auth.joinTeam(created.id, role, displayName)
    teamDetails.value = { ...teamDetails.value, [created.id]: withCrest }
    wizardOpen.value = false
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

/** Snapshot of the team being left, captured when the dialog opens - kept stable
 *  through the dialog's closing transition even after `teamDetails` is pruned. */
const leavingTeam = ref<Team | null>(null)
const leaveError = ref('')

async function confirmLeave() {
  if (!leavingTeam.value) return
  const teamId = leavingTeam.value.id
  leaveError.value = ''
  try {
    await auth.leaveTeam(teamId)
    const { [teamId]: _removed, ...rest } = teamDetails.value
    teamDetails.value = rest
    leavingTeam.value = null
  } catch (error) {
    leaveError.value = error instanceof ApiError ? error.message : t('profile.team.leaveFailed')
  }
}
</script>

<template>
  <v-main>
    <v-container v-if="user" class="my-teams-container">
      <!-- The page title moved to the breadcrumb (see AppShell). -->
      <div v-if="myTeams.length === 0" class="fp-alert fp-alert-warning mb-5">
        {{ t('profile.activation.needsTeam') }}
      </div>

      <v-row>
        <!-- Left: join an existing team, and found a new one. Always available -
             no toggle needed to reveal it. -->
        <v-col cols="12" md="4" class="py-6">
          <v-card class="fp-card pa-6">
            <h2 class="fp-section-title mb-4">{{ t('profile.team.joinTitle') }}</h2>

            <v-row dense>
              <v-col cols="12" sm="6">
                <FlatField :label="t('profile.team.select')" :error="joinErrors.team" class="mb-4">
                  <v-autocomplete
                    v-model="selectedTeamId"
                    :items="joinableTeams"
                    item-title="name"
                    item-value="id"
                    :placeholder="t('profile.team.selectPlaceholder')"
                    variant="outlined"
                    density="compact"
                    hide-details
                    clearable
                    :error="!!joinErrors.team"
                    class="fp-autocomplete"
                  />
                </FlatField>
              </v-col>
              <v-col cols="12" sm="6">
                <FlatField :label="t('profile.team.displayName')" :error="joinErrors.displayName" class="mb-4">
                  <input
                    v-model="joinDisplayName"
                    class="fp-input"
                    :class="{ 'fp-invalid': joinErrors.displayName }"
                    type="text"
                  />
                </FlatField>
              </v-col>
            </v-row>

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

            <FlatField :label="t('profile.team.memberRole')" :error="joinErrors.role" class="mb-4">
              <RolePills v-model="joinRole" />
            </FlatField>

            <div v-if="teamError" class="fp-alert fp-alert-error my-4">{{ teamError }}</div>
            <div v-else-if="teamSaved" class="fp-alert fp-alert-success my-4">
              {{ t('profile.team.saved') }}
            </div>

            <button
              type="button"
              class="fp-btn fp-btn-tonal fp-btn-block"
              :disabled="savingTeam"
              @click="saveTeam"
            >
              <v-progress-circular v-if="savingTeam" indeterminate size="16" width="2" color="white" />
              <template v-else>{{ t('profile.team.save') }}</template>
            </button>

            <!-- Create a new team: always available, opens the step-by-step wizard. -->
            <button
              type="button"
              class="fp-btn fp-btn-solid fp-btn-block mt-3"
              @click="wizardOpen = true"
            >
              <v-icon :icon="mdiPlusCircleOutline" size="18" />
              {{ t('profile.team.createAnother') }}
            </button>
          </v-card>
        </v-col>

        <!-- Right: the member's own teams, in a scrollable panel once the list grows. -->
        <v-col v-if="myTeams.length > 0" cols="12" md="8" class="py-6">
          <div class="fp-team-list">
            <v-card v-for="{ membership, team } in myTeamCards" :key="membership.id" class="fp-card fp-team-card">
              <div class="fp-crest-shield">
                <img v-if="team.hasCrest" :src="teamsApi.crestUrl(team.id)" :alt="team.name" />
                <v-icon v-else :icon="mdiShieldOutline" size="34" color="primary" />
              </div>
              <div class="fp-team-body">
                <div class="fp-team-row1">
                  <span class="fp-team-name">{{ team.name }}</span>
                  <span class="fp-role-chip">{{ t(`profile.team.memberRoles.${membership.role}`) }}</span>
                </div>
                <div class="fp-chip-row">
                  <span class="fp-chip" :style="tonalStyle(MODALITY_COLOR[team.type])">
                    {{ t(`profile.team.enums.${team.type}`) }}
                  </span>
                  <span class="fp-chip" :style="tonalStyle(DIVISION_COLOR[team.division])">
                    {{ t(`profile.team.enums.${team.division}`) }}
                  </span>
                  <span class="fp-chip" :style="tonalStyle(AGE_CATEGORY_COLOR[team.category])">
                    {{ t(`profile.team.enums.${team.category}`) }}
                  </span>
                  <span class="fp-chip" style="background: #f1f5f9; color: #475569">{{ team.city }}</span>
                </div>
                <div class="fp-meta-row">
                  <span>
                    <v-icon size="14" :icon="mdiAccountOutline" />
                    {{ membership.displayName }}
                  </span>
                  <span v-if="team.venueName">
                    <v-icon size="14" :icon="mdiSoccerField" />
                    {{ team.venueName }}
                  </span>
                </div>
              </div>
              <div class="fp-team-actions">
                <v-tooltip :text="t('profile.team.viewDetails')" location="top">
                  <template #activator="{ props: tooltipProps }">
                    <router-link
                      v-bind="tooltipProps"
                      :to="{ name: 'team-detail', params: { id: team.id } }"
                      class="fp-icon-btn"
                      :aria-label="t('profile.team.viewDetails')"
                      @click.stop
                    >
                      <v-icon :icon="mdiCardAccountDetailsOutline" size="18" />
                    </router-link>
                  </template>
                </v-tooltip>
                <v-tooltip :text="t('profile.team.editTitle')" location="top">
                  <template #activator="{ props: tooltipProps }">
                    <button
                      type="button"
                      v-bind="tooltipProps"
                      class="fp-icon-btn"
                      :aria-label="t('profile.team.editTitle')"
                      @click="openEdit(team.id)"
                    >
                      <v-icon :icon="mdiPencilOutline" size="18" />
                    </button>
                  </template>
                </v-tooltip>
                <v-tooltip :text="t('profile.team.leave')" location="top">
                  <template #activator="{ props: tooltipProps }">
                    <button
                      type="button"
                      v-bind="tooltipProps"
                      class="fp-icon-btn fp-icon-btn--danger"
                      :aria-label="t('profile.team.leave')"
                      @click="leavingTeam = team"
                    >
                      <v-icon :icon="mdiAccountRemoveOutline" size="18" />
                    </button>
                  </template>
                </v-tooltip>
              </div>
            </v-card>
          </div>
        </v-col>
      </v-row>
    </v-container>

    <!-- Create a new team -->
    <TeamWizard
      v-model="wizardOpen"
      :loading="creatingTeam"
      :error="createError"
      @submit="handleCreate"
    />

    <!-- Edit a team -->
    <v-dialog :model-value="!!editingTeam" max-width="560" scrollable @update:model-value="editingTeamId = null">
      <v-card v-if="editingTeam" class="fp-card fp-modal-card pa-6">
        <h2 class="fp-confirm-title" style="margin-bottom: 16px">{{ t('profile.team.editTitle') }}</h2>
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
    <v-dialog :model-value="!!leavingTeam" max-width="400" @update:model-value="leavingTeam = null">
      <v-card v-if="leavingTeam" class="fp-card fp-modal-card pa-6">
        <h2 class="fp-confirm-title">
          {{ t('profile.team.leaveConfirmTitle', { name: leavingTeam.name }) }}
        </h2>
        <p class="fp-confirm-hint">{{ t('profile.team.leaveConfirmHint') }}</p>
        <div v-if="leaveError" class="fp-alert fp-alert-error mb-4">{{ leaveError }}</div>
        <div class="fp-confirm-actions">
          <button type="button" class="fp-btn fp-btn-text" @click="leavingTeam = null">
            {{ t('profile.team.cancel') }}
          </button>
          <button type="button" class="fp-btn fp-btn-danger" @click="confirmLeave">
            {{ t('profile.team.leave') }}
          </button>
        </div>
      </v-card>
    </v-dialog>
  </v-main>
</template>

<style scoped>
/* Matches the max-width of the other list pages (Teams, Standings), now that
   this view is a two-column layout rather than a single narrow form. */
.my-teams-container {
  max-width: 1600px;
}

/* Bounded so a long team list scrolls in place instead of pushing the page
   footer away - the left column (join/create) stays put beside it. */
.fp-team-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: 70vh;
  overflow-y: auto;
  padding-right: 4px;
}
</style>
