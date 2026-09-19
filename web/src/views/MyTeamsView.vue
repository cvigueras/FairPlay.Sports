<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiAccountOutline,
  mdiAccountPlusOutline,
  mdiAccountRemoveOutline,
  mdiCardAccountDetailsOutline,
  mdiChevronDown,
  mdiClose,
  mdiPencilOutline,
  mdiPlusCircleOutline,
  mdiShieldOutline,
  mdiSoccerField,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import FlatField from '@/components/FlatField.vue'
import RolePills from '@/components/RolePills.vue'
import TeamCrest from '@/components/TeamCrest.vue'
import TeamWizard from '@/components/TeamWizard.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { DIVISION_COLOR } from '@/lib/division'
import { MEMBER_ROLE_COLOR } from '@/lib/memberRole'
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
   the auth store; the actual Team records are fetched by id here. */

const teamDetails = ref<Record<string, Team>>({})
const loadingMyTeams = ref(false)
const selectedTeamId = ref<string | null>(null)
const selectedTeam = ref<Team | null>(null)
const joinRole = ref<TeamMemberRole | null>(null)
/** Fixed to the account's username - joining a team always uses it, not a
 *  per-team nickname (that's what the create wizard's own name field is for). */
const joinDisplayName = ref(auth.currentUser?.userName ?? '')
const savingTeam = ref(false)
const joinErrors = reactive<Record<string, string>>({})
/** Mobile only (see .fp-join-mobile below): the join form starts collapsed
 *  behind a toggle button instead of always open, so a narrow screen shows
 *  the member's own teams first. Unused on desktop, where the form stays
 *  permanently visible regardless of this flag. */
const joinPanelOpen = ref(false)

/* ---- Join an existing team ----------------------------------------------
   The roster easily runs past any list-everything page size, so the picker
   searches the backend by name as the user types instead of holding one
   flat list of every team - it never has to fully overlap with `myTeams`. */

const TEAM_SEARCH_DEBOUNCE_MS = 300
const teamSearch = ref('')
const joinableTeams = ref<Team[]>([])
const searchingTeams = ref(false)

async function searchJoinableTeams() {
  searchingTeams.value = true
  try {
    const page = await teamsApi.page(
      { name: teamSearch.value.trim() || undefined, pageSize: 20, sort: 'name' },
      auth.accessToken,
    )
    joinableTeams.value = page.items.filter(
      (team) => !myTeams.value.some((membership) => membership.teamId === team.id),
    )
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.loadFailed'), 'error')
  } finally {
    searchingTeams.value = false
  }
}

let teamSearchTimer: ReturnType<typeof setTimeout> | undefined
watch(teamSearch, () => {
  clearTimeout(teamSearchTimer)
  teamSearchTimer = setTimeout(searchJoinableTeams, TEAM_SEARCH_DEBOUNCE_MS)
})

watch(selectedTeamId, (id) => {
  selectedTeam.value = id ? (joinableTeams.value.find((team) => team.id === id) ?? selectedTeam.value) : null
})

/** Each membership paired with its resolved `Team`, once fetched. Newest
 *  membership first, so a just-created (and just-joined) team lands at the
 *  top of the list rather than wherever the backend happens to return it. */
const myTeamCards = computed(() =>
  myTeams.value
    .map((membership) => ({ membership, team: teamDetails.value[membership.teamId] }))
    .filter((card): card is { membership: TeamMembership; team: Team } => !!card.team)
    .sort((a, b) => new Date(b.membership.createdAt).getTime() - new Date(a.membership.createdAt).getTime()),
)

/** Mobile only (see .fp-team-accordion below): which rows currently have
 *  their details expanded, keyed by membership id - mirrors TeamsView's own
 *  expand-in-place row pattern. */
const expandedTeamRows = reactive<Record<string, boolean>>({})

function toggleTeamRow(id: string) {
  expandedTeamRows[id] = !expandedTeamRows[id]
}

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
  try {
    await auth.loadMyTeams()
  } catch (error) {
    ui.notify(error instanceof ApiError ? error.message : t('profile.team.loadFailed'), 'error')
  }

  await Promise.all([searchJoinableTeams(), loadMyTeamDetails()])
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
    await Promise.all([loadMyTeamDetails(), searchJoinableTeams()])
    ui.notify(t('profile.team.saved'), 'success')
    selectedTeamId.value = null
    selectedTeam.value = null
    joinRole.value = null
    teamSearch.value = ''
    joinPanelOpen.value = false
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
        <v-col cols="12" md="4" class="py-6 my-teams-join-col">
          <v-card class="fp-card pa-6 fp-join-card">
            <h2 class="fp-section-title mb-4">{{ t('profile.team.joinTitle') }}</h2>

            <FlatField :label="t('profile.team.displayName')" class="mb-4">
              <input v-model="joinDisplayName" class="fp-input" type="text" disabled />
            </FlatField>

            <FlatField :label="t('profile.team.select')" :error="joinErrors.team" required class="mb-4">
              <v-autocomplete
                v-model="selectedTeamId"
                v-model:search="teamSearch"
                :items="joinableTeams"
                :loading="searchingTeams"
                no-filter
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
                      <template v-if="item.division">{{ t(`profile.team.enums.${item.division}`) }} · </template>
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

            <FlatField :label="t('profile.team.memberRole')" :error="joinErrors.role" required class="mb-4">
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

          <!-- Mobile only (see the max-width: 959.98px rules below, which
               hide .fp-join-card above instead): the same join form, but
               collapsed behind a toggle button so a narrow screen leads with
               the member's own teams, not a form. -->
          <div class="fp-join-mobile">
            <button
              type="button"
              class="fp-btn fp-btn-tonal fp-btn-block fp-join-toggle"
              :class="{ 'fp-join-toggle--open': joinPanelOpen }"
              :aria-expanded="joinPanelOpen"
              @click="joinPanelOpen = !joinPanelOpen"
            >
              <v-icon :icon="mdiAccountPlusOutline" size="18" />
              {{ t('profile.team.joinTitle') }}
              <v-icon :icon="mdiChevronDown" size="18" class="fp-join-toggle-chevron" />
            </button>

            <v-card v-if="joinPanelOpen" class="fp-card pa-4 mt-3">
              <div class="d-flex align-center justify-space-between mb-4">
                <h2 class="fp-section-title">{{ t('profile.team.joinTitle') }}</h2>
                <button
                  type="button"
                  class="fp-icon-btn"
                  :aria-label="t('profile.team.cancel')"
                  @click="joinPanelOpen = false"
                >
                  <v-icon :icon="mdiClose" size="16" />
                </button>
              </div>

              <FlatField :label="t('profile.team.displayName')" class="mb-4">
                <input v-model="joinDisplayName" class="fp-input" type="text" disabled />
              </FlatField>

              <FlatField :label="t('profile.team.select')" :error="joinErrors.team" required class="mb-4">
                <v-autocomplete
                  v-model="selectedTeamId"
                  v-model:search="teamSearch"
                  :items="joinableTeams"
                  :loading="searchingTeams"
                  no-filter
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
                        <template v-if="item.division">{{ t(`profile.team.enums.${item.division}`) }} · </template>
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

              <FlatField :label="t('profile.team.memberRole')" :error="joinErrors.role" required class="mb-4">
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
            </v-card>

            <!-- Create a new team: always available, independent of the
                 join panel above. -->
            <button
              type="button"
              class="fp-btn fp-btn-solid fp-btn-block mt-3"
              @click="wizardOpen = true"
            >
              <v-icon :icon="mdiPlusCircleOutline" size="18" />
              {{ t('profile.team.createAnother') }}
            </button>
          </div>
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
                  <span class="fp-role-chip" :style="tonalStyle(MEMBER_ROLE_COLOR[membership.role])">
                    {{ t(`profile.team.memberRoles.${membership.role}`) }}
                  </span>
                </div>
                <div class="fp-chip-row">
                  <span class="fp-chip" :style="tonalStyle(MODALITY_COLOR[team.type])">
                    {{ t(`profile.team.enums.${team.type}`) }}
                  </span>
                  <span v-if="team.division" class="fp-chip" :style="tonalStyle(DIVISION_COLOR[team.division])">
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

          <!-- Mobile only (see the max-width: 959.98px rules below, which
               swap this in for .fp-team-list above): one row per team -
               crest, name, your role - that expands in place instead of the
               desktop card's always-open layout, matching the Teams
               screen's own mobile row/expand pattern. -->
          <div class="fp-team-accordion">
            <v-card class="fp-card fp-accordion-card">
              <template v-for="{ membership, team } in myTeamCards" :key="membership.id">
                <button
                  type="button"
                  class="fp-accordion-row"
                  :aria-expanded="!!expandedTeamRows[membership.id]"
                  @click="toggleTeamRow(membership.id)"
                >
                  <TeamCrest :team="team" :size="32" />
                  <span class="fp-accordion-name text-truncate">{{ team.name }}</span>
                  <span class="fp-role-chip fp-role-chip--sm" :style="tonalStyle(MEMBER_ROLE_COLOR[membership.role])">
                    {{ t(`profile.team.memberRoles.${membership.role}`) }}
                  </span>
                  <v-icon
                    :icon="mdiChevronDown"
                    size="18"
                    class="fp-accordion-chevron"
                    :class="{ 'fp-accordion-chevron--open': expandedTeamRows[membership.id] }"
                  />
                </button>

                <div v-if="expandedTeamRows[membership.id]" class="fp-accordion-details">
                  <div class="fp-accordion-grid">
                    <div class="fp-accordion-cell">
                      <span class="fp-accordion-label">{{ t('profile.team.type') }}</span>
                      <span class="fp-chip" :style="tonalStyle(MODALITY_COLOR[team.type])">
                        {{ t(`profile.team.enums.${team.type}`) }}
                      </span>
                    </div>
                    <div class="fp-accordion-cell">
                      <span class="fp-accordion-label">{{ t('profile.team.division') }}</span>
                      <span v-if="team.division" class="fp-chip" :style="tonalStyle(DIVISION_COLOR[team.division])">
                        {{ t(`profile.team.enums.${team.division}`) }}
                      </span>
                      <span v-else class="text-body-2 text-medium-emphasis">—</span>
                    </div>
                    <div class="fp-accordion-cell">
                      <span class="fp-accordion-label">{{ t('profile.team.category') }}</span>
                      <span class="fp-chip" :style="tonalStyle(AGE_CATEGORY_COLOR[team.category])">
                        {{ t(`profile.team.enums.${team.category}`) }}
                      </span>
                    </div>
                    <div class="fp-accordion-cell">
                      <span class="fp-accordion-label">{{ t('profile.team.city') }}</span>
                      <span class="fp-accordion-value">{{ team.city }}</span>
                    </div>
                  </div>

                  <div class="fp-accordion-cell">
                    <span class="fp-accordion-label">{{ t('profile.team.displayName') }}</span>
                    <span class="fp-accordion-value">
                      <v-icon size="14" :icon="mdiAccountOutline" />
                      {{ membership.displayName }}
                    </span>
                  </div>

                  <div v-if="team.venueName" class="fp-accordion-cell">
                    <span class="fp-accordion-label">{{ t('profile.team.venueGroup') }}</span>
                    <span class="fp-accordion-value">
                      <v-icon size="14" :icon="mdiSoccerField" />
                      {{ team.venueName }}
                    </span>
                  </div>

                  <div class="fp-accordion-actions">
                    <router-link
                      :to="{ name: 'team-detail', params: { id: team.id } }"
                      class="fp-accordion-action"
                      :aria-label="t('profile.team.viewDetails')"
                      @click.stop
                    >
                      <v-icon :icon="mdiCardAccountDetailsOutline" size="18" />
                      {{ t('profile.team.viewDetails') }}
                    </router-link>
                    <button type="button" class="fp-accordion-action" @click="openEdit(team.id)">
                      <v-icon :icon="mdiPencilOutline" size="18" />
                      {{ t('profile.team.editTitle') }}
                    </button>
                    <button
                      type="button"
                      class="fp-accordion-action fp-accordion-action--danger"
                      @click="leavingTeam = team"
                    >
                      <v-icon :icon="mdiAccountRemoveOutline" size="18" />
                      {{ t('profile.team.leave') }}
                    </button>
                  </div>
                </div>
              </template>
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

/* Mobile-only replacement for .fp-team-list above (see the max-width:
   959.98px rules below, which swap the two): one row per team that expands
   in place, matching the Teams screen's own mobile pattern, instead of the
   desktop card's always-open layout - hidden here so it never renders
   twice while both exist unconditionally in the template. */
.fp-team-accordion {
  display: none;
}

/* Mobile-only replacement for .fp-join-card above (see the max-width:
   959.98px rules below, which swap the two): the join form starts
   collapsed behind a toggle button instead of always open. */
.fp-join-mobile {
  display: none;
}

/* Positioned absolutely (rather than pushed right with margin-left: auto)
   so it doesn't eat the flex row's free space - an auto margin there
   claims it all for itself, leaving .fp-btn's justify-content: center
   nothing left to center the icon+label against, so the text reads
   left-aligned instead. */
.fp-join-toggle {
  position: relative;
}

.fp-join-toggle-chevron {
  position: absolute;
  right: 16px;
  top: 50%;
  transform: translateY(-50%);
  transition: transform 0.15s ease;
}

.fp-join-toggle--open .fp-join-toggle-chevron {
  transform: translateY(-50%) rotate(180deg);
}

.fp-accordion-card {
  overflow: hidden;
}

.fp-accordion-row {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  box-sizing: border-box;
  padding: 12px 14px;
  border: none;
  border-bottom: 1px solid #f1f5f9;
  background: rgb(var(--v-theme-surface));
  font: inherit;
  text-align: left;
  cursor: pointer;
  min-height: 44px;
}

.fp-accordion-name {
  flex: 1;
  min-width: 0;
  font-weight: 700;
  font-size: 15px;
  color: #0f172a;
}

.fp-role-chip--sm {
  padding: 3px 10px;
  font-size: 11.5px;
  flex-shrink: 0;
}

.fp-accordion-chevron {
  flex-shrink: 0;
  color: #94a3b8;
  transition: transform 0.15s ease;
}

.fp-accordion-chevron--open {
  transform: rotate(180deg);
}

.fp-accordion-details {
  padding: 14px;
  border-bottom: 1px solid #f1f5f9;
  background: rgb(var(--v-theme-background));
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.fp-accordion-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.fp-accordion-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.fp-accordion-cell .fp-chip {
  align-self: flex-start;
}

.fp-accordion-label {
  font-size: 10.5px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.fp-accordion-value {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13.5px;
  font-weight: 600;
  color: #334155;
}

.fp-accordion-actions {
  display: flex;
  gap: 8px;
  margin-top: 2px;
}

.fp-accordion-action {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  height: 52px;
  border-radius: 10px;
  border: 1.5px solid #e2e8f0;
  background: rgb(var(--v-theme-surface));
  color: #334155;
  font-size: 10.5px;
  font-weight: 700;
  text-decoration: none;
  text-align: center;
  cursor: pointer;
}

.fp-accordion-action--danger {
  color: rgb(var(--v-theme-error));
  border-color: #fecaca;
}

/* Mobile only (below Vuetify's `md` breakpoint, where the two columns stack
   instead of sitting side by side): the desktop layout above pins the row to
   the viewport height and hides its overflow so only .fp-team-list scrolls
   internally. Stacked on one narrow column, that used to clip the team list
   below the join form with no way to reach it - "no funciona el scroll para
   abajo". Below `md` the row goes back to natural page height/scroll
   instead; the join form (top, natural DOM order) collapses behind
   .fp-join-mobile's toggle instead of .fp-join-card's always-open form, and
   the accordion list replaces the always-open desktop team cards. */
@media (max-width: 959.98px) {
  .my-teams-container {
    padding-block: 16px;
  }

  .my-teams-row {
    height: auto;
    overflow: visible;
  }

  .my-teams-list-col {
    height: auto;
    padding-top: 0;
  }

  .my-teams-join-col {
    padding-top: 8px;
    padding-bottom: 0;
  }

  .fp-join-card {
    display: none;
  }

  .fp-join-mobile {
    display: block;
  }

  .fp-team-list {
    display: none;
  }

  .fp-team-accordion {
    display: block;
  }
}
</style>
