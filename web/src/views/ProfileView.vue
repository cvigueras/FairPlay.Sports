<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  mdiClipboardTextOutline,
  mdiEyeOutline,
  mdiMapMarkerOutline,
  mdiPencilOutline,
  mdiSoccerField,
  mdiSwordCross,
} from '@mdi/js'
import { ApiError } from '@/lib/http'
import { teamsApi } from '@/lib/teams'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import ModalityIcon from '@/components/ModalityIcon.vue'
import TeamForm from '@/components/TeamForm.vue'
import { AGE_CATEGORY_COLOR } from '@/lib/ageCategory'
import { useAuthStore } from '@/stores/auth'
import { FOOTBALL_TYPES, type CreateTeamPayload, type Team } from '@/types/team'

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

type TeamDetailKind = 'coach' | 'venue'

const teamDetails = computed(() => {
  const team = myTeam.value
  if (!team) return []
  const rows: { label: string; value: string; kind: TeamDetailKind }[] = [
    { label: t('profile.team.coach'), value: team.coach, kind: 'coach' },
  ]
  if (team.venueName)
    rows.push({ label: t('profile.team.venueGroup'), value: team.venueName, kind: 'venue' })
  return rows
})

const detailIcon: Record<TeamDetailKind, string> = {
  coach: mdiEyeOutline,
  venue: mdiSoccerField,
}

/* Info modal: a single detail row, or the whole club sheet. */
type InfoKind = TeamDetailKind | 'sheet'
const infoKind = ref<InfoKind | null>(null)
const infoOpen = computed({
  get: () => infoKind.value !== null,
  set: (open: boolean) => {
    if (!open) infoKind.value = null
  },
})

function challengeTeam() {
  // TODO: wire up the team-vs-team challenge flow.
}

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

/** "How to get there": the club's own maps link, else a maps search of the venue. */
const venueMapsHref = computed(() => {
  const team = myTeam.value
  if (!team) return null
  if (team.venueMapsUrl) return team.venueMapsUrl
  const query = [team.venueName, team.venueAddress].filter(Boolean).join(', ')
  return query
    ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`
    : null
})
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
              class="px-4 py-4 px-md-8 py-md-0 d-flex ga-4 ga-md-6 team-panel"
            >
              <div class="team-panel-actions">
                <v-btn
                  size="small"
                  variant="text"
                  :prepend-icon="mdiPencilOutline"
                  @click="editOpen = true"
                >
                  {{ t('common.edit') }}
                </v-btn>
              </div>

              <div
                class="d-flex flex-column align-center justify-center flex-shrink-0 ga-1 team-identity"
              >
                <div class="team-name-row">
                  <ModalityIcon :type="myTeam.type" :size="36" />
                  <div class="team-name-col">
                    <p class="text-h6 font-weight-bold team-name mb-0">{{ myTeam.name }}</p>
                    <span
                      class="text-body-2 font-weight-bold"
                      :style="{ color: AGE_CATEGORY_COLOR[myTeam.category] }"
                    >
                      {{ t(`profile.team.enums.${myTeam.category}`) }}
                    </span>
                  </div>
                </div>
                <div class="team-identity-rule"></div>
                <div class="team-meta">
                  <div class="team-meta-division">
                    <div class="team-meta-label text-medium-emphasis">
                      {{ t('profile.team.division') }}
                    </div>
                    <div class="team-meta-bar"></div>
                    <div class="team-meta-value font-weight-medium">
                      {{ t(`profile.team.enums.${myTeam.division}`) }}
                    </div>
                  </div>
                  <span class="team-meta-city text-medium-emphasis">{{ myTeam.city }}</span>
                </div>
              </div>

              <v-divider vertical class="d-none d-md-block team-panel-divider" />

              <div class="flex-grow-1 team-detail-grid">
                <div class="team-detail-cells">
                  <div
                    v-for="row in teamDetails"
                    :key="row.label"
                    class="team-detail-cell"
                  >
                    <div class="d-flex align-center justify-space-between team-detail-head">
                      <span class="text-caption text-medium-emphasis">{{ row.label }}</span>
                      <v-btn
                        :icon="detailIcon[row.kind]"
                        variant="text"
                        size="x-small"
                        density="comfortable"
                        :aria-label="t('profile.team.viewDetails')"
                        @click="infoKind = row.kind"
                      />
                    </div>
                    <div class="text-body-1 font-weight-medium">{{ row.value }}</div>
                  </div>
                </div>

                <div class="team-detail-side">
                  <v-btn
                    color="red"
                    variant="flat"
                    size="small"
                    :prepend-icon="mdiSwordCross"
                    @click="challengeTeam"
                  >
                    {{ t('profile.team.challenge') }}
                  </v-btn>
                  <v-btn
                    color="blue"
                    variant="tonal"
                    size="small"
                    :prepend-icon="mdiClipboardTextOutline"
                    @click="infoKind = 'sheet'"
                  >
                    {{ t('profile.team.viewSheet') }}
                  </v-btn>
                </div>
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

        <!-- TEMP: preview of every modality icon -->
        <v-row>
          <v-col cols="12">
            <v-card
              border
              flat
              rounded="xl"
              class="pa-4 pa-sm-6 d-flex flex-wrap justify-center ga-6 ga-sm-10"
            >
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

    <!-- Detail info modal (eye / field buttons) -->
    <v-dialog v-model="infoOpen" max-width="420">
      <v-card v-if="myTeam" border flat rounded="xl" class="pa-6">
        <template v-if="infoKind === 'coach'">
          <h3 class="text-h6 font-weight-bold mb-3">{{ t('profile.team.coach') }}</h3>
          <p class="text-body-1">{{ myTeam.coach }}</p>
        </template>

        <template v-else-if="infoKind === 'venue'">
          <h3 class="text-h6 font-weight-bold mb-3">{{ t('profile.team.venueGroup') }}</h3>
          <dl class="team-info-dl">
            <dt>{{ t('profile.team.venueName') }}</dt>
            <dd>{{ myTeam.venueName }}</dd>
            <template v-if="myTeam.venueAddress">
              <dt>{{ t('profile.team.venueAddress') }}</dt>
              <dd>{{ myTeam.venueAddress }}</dd>
            </template>
            <template v-if="myTeam.venueSurface">
              <dt>{{ t('profile.team.venueSurface') }}</dt>
              <dd>{{ t(`profile.team.surfaces.${myTeam.venueSurface}`) }}</dd>
            </template>
          </dl>
          <v-btn
            v-if="venueMapsHref"
            :href="venueMapsHref"
            target="_blank"
            rel="noopener"
            variant="tonal"
            size="small"
            :prepend-icon="mdiMapMarkerOutline"
            class="mt-3"
          >
            {{ t('profile.team.directions') }}
          </v-btn>
        </template>

        <template v-else-if="infoKind === 'sheet'">
          <h3 class="text-h6 font-weight-bold mb-3">
            {{ myTeam.name }}<span v-if="myTeam.shortName"> · {{ myTeam.shortName }}</span>
          </h3>
          <dl class="team-info-dl">
            <dt>{{ t('profile.team.category') }}</dt>
            <dd>{{ t(`profile.team.enums.${myTeam.category}`) }}</dd>
            <dt>{{ t('profile.team.type') }}</dt>
            <dd>{{ t(`profile.team.enums.${myTeam.type}`) }}</dd>
            <dt>{{ t('profile.team.division') }}</dt>
            <dd>{{ t(`profile.team.enums.${myTeam.division}`) }}</dd>
            <dt>{{ t('profile.team.city') }}</dt>
            <dd>{{ myTeam.city }}</dd>
            <dt>{{ t('profile.team.coach') }}</dt>
            <dd>{{ myTeam.coach }}</dd>
            <template v-if="myTeam.foundedYear">
              <dt>{{ t('profile.team.foundedYear') }}</dt>
              <dd>{{ myTeam.foundedYear }}</dd>
            </template>
            <template v-if="myTeam.venueName">
              <dt>{{ t('profile.team.venueGroup') }}</dt>
              <dd>{{ myTeam.venueName }}</dd>
            </template>
            <template v-if="myTeam.colorPrimary && myTeam.colorSecondary">
              <dt>{{ t('profile.team.colorsGroup') }}</dt>
              <dd>{{ myTeam.colorPrimary }} / {{ myTeam.colorSecondary }}</dd>
            </template>
            <template v-if="myTeam.contactEmail">
              <dt>{{ t('profile.team.contactEmail') }}</dt>
              <dd>{{ myTeam.contactEmail }}</dd>
            </template>
            <template v-if="myTeam.contactPhone">
              <dt>{{ t('profile.team.contactPhone') }}</dt>
              <dd>{{ myTeam.contactPhone }}</dd>
            </template>
            <template v-if="myTeam.website">
              <dt>{{ t('profile.team.website') }}</dt>
              <dd>{{ myTeam.website }}</dd>
            </template>
          </dl>
        </template>

        <div class="d-flex justify-end mt-4">
          <v-btn variant="text" @click="infoKind = null">{{ t('common.close') }}</v-btn>
        </div>
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

/* Division block (label + green rule + value) on the left, city on the right. */
.team-meta {
  align-self: stretch;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.team-meta-division {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  text-align: center;
  min-width: 90px;
}

.team-meta-label {
  font-size: 0.8rem;
  line-height: 1.15;
}

.team-meta-bar {
  height: 3px;
  margin: 2px 0;
  border-radius: 2px;
  background: #86efac;
}

.team-meta-value {
  font-size: 0.9rem;
  line-height: 1.2;
}

.team-meta-city {
  margin-left: auto;
  font-size: 0.9rem;
  line-height: 1.2;
  text-align: right;
}

/* Team panel — mobile-first: the identity block and the details stack, each
   full width. The side-by-side "shaded strip + rule + grid" layout kicks in
   from the md breakpoint (see the media query at the end). */
.team-panel {
  overflow: hidden;
  position: relative;
  flex-direction: column;
  align-items: stretch;
}

.team-panel-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.25rem;
}

.team-identity {
  border-radius: 12px;
  padding: 1rem;
  background: rgba(var(--v-theme-on-surface), 0.04);
}

/* White rule where the crest used to be - full-bleed across the shaded strip. */
.team-identity-rule {
  align-self: stretch;
  height: 2px;
  margin: 0.05rem -1rem;
  background: rgb(var(--v-theme-surface));
}

.team-panel-divider {
  align-self: stretch;
  /* Let flex stretch size it so the negative margins add the card padding
     back on; Vuetify's own height/max-height would otherwise cap it. */
  height: auto;
  max-height: none;
  margin-block: 0;
}

.team-name {
  line-height: 1.25;
  overflow-wrap: anywhere;
}

/* Icon pinned left (in line with the division block); name/category
   right-anchored so a longer club name grows leftwards, its right edge
   lining up with the city value below. */
.team-name-row {
  align-self: stretch;
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
}

.team-name-col {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  text-align: right;
  gap: 2px;
  /* Nudge the club name / category up to sit against the icon's top. */
  margin-top: -0.35rem;
}

.team-name-row .team-name {
  line-height: 1.05;
}

.team-detail-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.5rem;
  align-content: center;
}

/* Coach + venue stacked as one block with collapsed shared borders. */
.team-detail-cells {
  display: flex;
  flex-direction: column;
}

.team-detail-cell {
  border: 0;
  padding: 0.2rem 0.85rem;
}

/* Coach keeps only a bottom rule, venue only a top rule; the -1px pull
   collapses the pair into one crisp divider between them. */
.team-detail-cells .team-detail-cell:first-child {
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.team-detail-cells .team-detail-cell + .team-detail-cell {
  margin-top: -1px;
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

/* Right-hand actions: "challenge" on top, "view sheet" below. */
.team-detail-side {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* Label + info button share one line, so the buttons line up across cells. */
.team-detail-head {
  min-height: 16px;
  margin: -2px -4px 0 0;
}

.team-info-dl {
  display: grid;
  grid-template-columns: auto 1fr;
  column-gap: 1rem;
  row-gap: 0.35rem;
  margin: 0;
}

.team-info-dl dt {
  font-size: 0.75rem;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.team-info-dl dd {
  margin: 0;
  font-weight: 500;
}

@media (min-width: 960px) {
  .team-panel {
    flex-direction: row;
    align-items: stretch;
  }

  .team-panel-actions {
    position: absolute;
    top: 0.5rem;
    right: 0.75rem;
    z-index: 1;
    flex-wrap: nowrap;
  }

  .team-identity-rule {
    margin-inline: -2rem;
  }

  .team-identity {
    width: 264px;
    align-self: stretch;
    border-radius: 0;
    /* Bleed to the card's top / left / bottom edges and up to the divider
       (card block padding is 0 at md; the right -1.5rem just cancels the
       flex gap). */
    margin: 0 -1.5rem 0 -2rem;
    padding: 0.15rem 2rem;
  }

  .team-detail-grid {
    grid-template-columns: repeat(2, 1fr);
    /* Cancel the flex gap after the divider so the coach / venue rows
       sit flush against the shaded identity strip. */
    margin-left: -1.5rem;
  }

  .team-detail-cells {
    /* Stacked in the left half; the right column holds the actions. */
    grid-column: 1;
  }

  .team-detail-cell {
    /* Flush left against the strip, just a hairline's clearance. */
    padding-left: 0.5rem;
  }

  .team-detail-side {
    grid-column: 2;
    /* Centred against the coach/venue stack, which drives the row height. */
    align-self: center;
    justify-content: center;
    align-items: center;
    /* Top inset clears the absolute Edit button; the column is still
       shorter than the cell stack so the card doesn't grow. */
    padding: 1.75rem 0 0.25rem;
  }
}
</style>
