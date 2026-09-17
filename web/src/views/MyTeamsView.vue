<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiAccountOutline,
  mdiAccountPlusOutline,
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
import TeamWizard from '@/components/TeamWizard.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MODALITY_COLOR } from '@/lib/modality'
import { tonalStyle } from '@/lib/tonalColor'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { CreateTeamPayload, Team, TeamMemberRole, TeamMembership } from '@/types/team'

const auth = useAuthStore()
const ui = useUiStore()
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
/** Fixed to the account's username - joining a team always uses it, not a
 *  per-team nickname (that's what the create wizard's own name field is for). */
const joinDisplayName = ref(auth.currentUser?.userName ?? '')
const savingTeam = ref(false)
const joinErrors = reactive<Record<string, string>>({})

const joinableTeams = computed(() =>
  teams.value.filter((team) => !myTeams.value.some((membership) => membership.teamId === team.id)),
)
const selectedTeam = computed(() => teams.value.find((team) => team.id === selectedTeamId.value) ?? null)

/** Each membership paired with its resolved `Team`, once fetched. Newest
 *  membership first, so a just-created (and just-joined) team lands at the
 *  top of the list rather than wherever the backend happens to return it. */
const myTeamCards = computed(() =>
  myTeams.value
    .map((membership) => ({ membership, team: teamDetails.value[membership.teamId] }))
    .filter((card): card is { membership: TeamMembership; team: Team } => !!card.team)
    .sort((a, b) => new Date(b.membership.createdAt).getTime() - new Date(a.membership.createdAt).getTime()),
)

async function loadMyTeamDetails() {
  loadingMyTeams.value = true
  try {
    const details = await Promise.all(
      myTeams.value.map((membership) => teamsApi.byId(membership.teamId, auth.accessToken)),
    )
    teamDetails.value = Object.fromEntries(details.map((team) => [team.id, team]))
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.loadFailed'), 'error')
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
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.loadFailed'), 'error')
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

  return Object.keys(joinErrors).length === 0
}

async function saveTeam() {
  if (!validateJoin() || !selectedTeamId.value || !joinRole.value) return
  savingTeam.value = true
  try {
    await auth.joinTeam(selectedTeamId.value, joinRole.value, joinDisplayName.value.trim())
    await loadMyTeamDetails()
    ui.notify(t('profile.team.saved'), 'success')
    selectedTeamId.value = null
    joinRole.value = null
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.saveFailed'), 'error')
  } finally {
    savingTeam.value = false
  }
}

/* ---- Create a new team (wizard) --------------------------------------- */

const wizardOpen = ref(false)
const creatingTeam = ref(false)

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
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.createFailed'), 'error')
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

function openEdit(teamId: string) {
  editingTeamId.value = teamId
}

async function handleUpdate({ payload, crest }: { payload: CreateTeamPayload; crest: File | null }) {
  if (!editingTeam.value) return
  savingEdit.value = true
  try {
    const updated = await teamsApi.update(editingTeam.value.id, payload, auth.accessToken)
    if (crest) await teamsApi.uploadCrest(updated.id, crest, auth.accessToken)

    const withCrest = crest ? { ...updated, hasCrest: true } : updated
    teamDetails.value = { ...teamDetails.value, [withCrest.id]: withCrest }
    teams.value = teams.value.map((tm) => (tm.id === withCrest.id ? withCrest : tm))
    editingTeamId.value = null
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.updateFailed'), 'error')
  } finally {
    savingEdit.value = false
  }
}

/* ---- Leave a team ------------------------------------------------------------ */

/** Snapshot of the team being left, captured when the dialog opens - kept stable
 *  through the dialog's closing transition even after `teamDetails` is pruned. */
const leavingTeam = ref<Team | null>(null)

async function confirmLeave() {
  if (!leavingTeam.value) return
  const teamId = leavingTeam.value.id
  try {
    await auth.leaveTeam(teamId)
    const { [teamId]: _removed, ...rest } = teamDetails.value
    teamDetails.value = rest
    leavingTeam.value = null
    ui.notify(t('profile.team.leftSuccess'), 'success')
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.leaveFailed'), 'error')
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

      <v-row class="my-teams-row">
        <!-- Left: join an existing team, and found a new one. Always available -
             no toggle needed to reveal it. -->
        <v-col cols="12" md="4" class="py-6">
          <v-card class="fp-card pa-6">
            <h2 class="fp-section-title mb-4">{{ t('profile.team.joinTitle') }}</h2>

            <FlatField :label="t('profile.team.displayName')" class="mb-4">
              <input v-model="joinDisplayName" class="fp-input" type="text" disabled />
            </FlatField>

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
                autocomplete="off"
                :error="!!joinErrors.team"
                class="fp-autocomplete"
              >
                <template #item="{ item, props: itemProps }">
                  <v-list-item v-bind="itemProps" :title="item.name">
                    <template #subtitle>
                      {{ t(`profile.team.enums.${item.type}`) }} ·
                      {{ t(`profile.team.enums.${item.division}`) }} ·
                      {{ t(`profile.team.enums.${item.category}`) }}
                    </template>
                  </v-list-item>
                </template>
              </v-autocomplete>
            </FlatField>

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

            <button
              type="button"
              class="fp-btn fp-btn-tonal fp-btn-block"
              :disabled="savingTeam"
              @click="saveTeam"
            >
              <v-progress-circular v-if="savingTeam" indeterminate size="16" width="2" color="white" />
              <template v-else>
                <v-icon :icon="mdiAccountPlusOutline" size="18" />
                {{ t('profile.team.save') }}
              </template>
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
        <v-col v-if="myTeams.length > 0" cols="12" md="8" class="py-6 d-flex flex-column my-teams-list-col">
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
    <TeamWizard v-model="wizardOpen" :loading="creatingTeam" @submit="handleCreate" />

    <!-- Edit a team: the same wizard as creation, pre-filled from the team
         being edited (see TeamWizard's `initial` prop). -->
    <TeamWizard
      :model-value="!!editingTeam"
      :initial="editingTeam"
      :loading="savingEdit"
      @update:model-value="(open) => { if (!open) editingTeamId = null }"
      @update="handleUpdate"
    />

    <!-- Confirm leaving a team -->
    <v-dialog :model-value="!!leavingTeam" max-width="400" @update:model-value="leavingTeam = null">
      <v-card v-if="leavingTeam" class="fp-card fp-modal-card pa-6">
        <h2 class="fp-confirm-title">
          {{ t('profile.team.leaveConfirmTitle', { name: leavingTeam.name }) }}
        </h2>
        <p class="fp-confirm-hint">{{ t('profile.team.leaveConfirmHint') }}</p>
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
  /* Vuetify's v-container adds 16px top/bottom padding by default; left as
     is, that's 32px more than the viewport-height row below leaves room
     for, which is exactly what was pushing the whole page into scrolling
     instead of only .fp-team-list. The columns' own py-6 already gives
     top/bottom breathing room, so the container doesn't need to add more. */
  padding-block: 0;
}

/* Pinned to the viewport height below the app bar (64px) - there's no
   pagination to cap the list at a guessed height, so it may as well use
   all the room the window actually gives it. A fixed `height` (not
   `min-height`) plus `overflow: hidden` keeps the row itself, and the join
   form beside the list, from ever growing past that and scrolling the
   whole page - only .fp-team-list's own overflow scrolls. */
.my-teams-row {
  height: calc(100vh - 64px);
  overflow: hidden;
}

/* Vuetify's v-col doesn't reliably stretch to a flex row's cross size (its
   own default flex-grow/shrink: 0 seems to win out even with min-height:
   0), so the column is pinned to the same explicit height as the row
   instead of depending on stretch to propagate it down. */
.my-teams-list-col {
  height: calc(100vh - 64px);
  min-height: 0;
}

.fp-team-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding-right: 4px;
}
</style>
