<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiAccountTieOutline, mdiCheckCircle, mdiCloseCircle, mdiLockOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import { MEMBER_ROLE_COLOR } from '@/lib/memberRole'
import { fullName } from '@/lib/userName'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const { t, locale } = useI18n()

const user = computed(() => auth.currentUser)

const formatLongDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'long', day: 'numeric' })

const memberSince = computed(() => (user.value ? formatLongDate(user.value.createdAt) : ''))

/** "First Last" when the account has them (older accounts fall back to the username). */
const displayName = computed(() => fullName(user.value))

/** The role picked at sign-up, in the active language. */
const primaryRoleLabel = computed(() =>
  user.value ? t(`profile.team.memberRoles.${user.value.primaryRole}`) : '',
)

/** Read-only account data, one row each. `—` where an older account has no value. */
const accountRows = computed(() => {
  if (!user.value) return []
  const u = user.value
  const names = [u.firstName, u.lastName].filter(Boolean).join(' ')
  return [
    { label: t('profile.account.fullName'), value: names || '—', breakAll: false },
    { label: t('profile.account.primaryRole'), value: primaryRoleLabel.value, breakAll: false },
    { label: t('profile.account.userName'), value: u.userName, breakAll: false },
    { label: t('profile.account.email'), value: u.email, breakAll: true },
  ]
})

/* ---- Security (change password) ----------------------------------------------- */

const passwordForm = reactive({ current: '', next: '', confirm: '' })
const passwordErrors = reactive({ current: '', next: '', confirm: '' })
const savingPassword = ref(false)
const passwordSaved = ref(false)
const passwordError = ref('')

function validatePassword(): boolean {
  passwordErrors.current = !passwordForm.current ? t('validation.passwordRequired') : ''
  passwordErrors.next = !passwordForm.next
    ? t('validation.passwordRequired')
    : passwordForm.next.length < 8
      ? t('validation.passwordMinLength')
      : ''
  passwordErrors.confirm = passwordForm.confirm !== passwordForm.next ? t('validation.passwordsMismatch') : ''

  return !passwordErrors.current && !passwordErrors.next && !passwordErrors.confirm
}

async function changePassword() {
  passwordError.value = ''
  passwordSaved.value = false
  if (!validatePassword()) return

  savingPassword.value = true
  try {
    await auth.changePassword({ currentPassword: passwordForm.current, newPassword: passwordForm.next })
    passwordSaved.value = true
    passwordForm.current = ''
    passwordForm.next = ''
    passwordForm.confirm = ''
  } catch (error) {
    passwordError.value = error instanceof ApiError ? error.message : t('profile.security.saveFailed')
  } finally {
    savingPassword.value = false
  }
}
</script>

<template>
  <v-main>
    <v-container v-if="user" class="py-6 py-md-10 profile-container">
      <!-- Identity: photo, username/email (read-only), role, account status, member-since. -->
      <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5">
        <div class="d-flex flex-column flex-sm-row align-center align-sm-start text-center text-sm-left ga-6">
          <ProfileAvatar />

          <div class="flex-grow-1 d-flex flex-column ga-2" style="min-width: 0">
            <div class="d-flex flex-column flex-sm-row align-center align-sm-baseline ga-sm-3">
              <h1 class="text-h5 font-weight-bold">{{ displayName }}</h1>
              <span class="text-body-2 text-medium-emphasis">{{ user.email }}</span>
            </div>

            <div class="d-flex align-center justify-center justify-sm-start ga-3 flex-wrap">
              <v-chip
                :color="MEMBER_ROLE_COLOR[user.primaryRole]"
                size="small"
                variant="tonal"
                :prepend-icon="mdiAccountTieOutline"
              >
                {{ primaryRoleLabel }}
              </v-chip>
              <v-chip :color="user.role === 'Admin' ? 'amber-darken-2' : 'primary'" size="small" variant="tonal">
                {{ user.role }}
              </v-chip>
              <v-chip :color="user.active ? 'success' : 'error'" size="small" variant="tonal">
                <v-icon :icon="user.active ? mdiCheckCircle : mdiCloseCircle" start size="16" />
                {{ user.active ? t('profile.status.active') : t('profile.status.inactive') }}
              </v-chip>
              <span class="text-caption text-medium-emphasis">
                {{ t('profile.fields.memberSince') }} {{ memberSince }}
              </span>
            </div>
          </div>
        </div>
      </v-card>

      <v-row>
        <!-- Account info: read-only, username/email can't be changed from here. -->
        <v-col cols="12" md="5">
          <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5 h-100">
            <h2 class="text-subtitle-1 font-weight-bold mb-5">{{ t('profile.account.title') }}</h2>

            <template v-for="(row, index) in accountRows" :key="row.label">
              <v-divider v-if="index > 0" class="my-5" />
              <div class="d-flex flex-column ga-1">
                <div class="d-flex align-center ga-2">
                  <span class="text-caption font-weight-medium text-medium-emphasis">{{ row.label }}</span>
                  <v-icon :icon="mdiLockOutline" size="14" color="medium-emphasis" />
                </div>
                <span class="text-body-1" :style="row.breakAll ? 'word-break: break-all' : undefined">
                  {{ row.value }}
                </span>
              </div>
            </template>
          </v-card>
        </v-col>

        <!-- Security: change password. -->
        <v-col cols="12" md="7">
          <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5 h-100">
            <h2 class="text-subtitle-1 font-weight-bold mb-4 d-flex align-center ga-2">
              <v-icon :icon="mdiLockOutline" />
              {{ t('profile.security.title') }}
            </h2>

            <v-row dense>
              <v-col cols="12" sm="4">
                <v-text-field
                  v-model="passwordForm.current"
                  type="password"
                  autocomplete="current-password"
                  :label="t('profile.security.currentPassword')"
                  :error-messages="passwordErrors.current"
                  hide-details="auto"
                />
              </v-col>
              <v-col cols="12" sm="4">
                <v-text-field
                  v-model="passwordForm.next"
                  type="password"
                  autocomplete="new-password"
                  :label="t('profile.security.newPassword')"
                  :error-messages="passwordErrors.next"
                  hide-details="auto"
                />
              </v-col>
              <v-col cols="12" sm="4">
                <v-text-field
                  v-model="passwordForm.confirm"
                  type="password"
                  autocomplete="new-password"
                  :label="t('profile.security.confirmPassword')"
                  :error-messages="passwordErrors.confirm"
                  hide-details="auto"
                />
              </v-col>
            </v-row>

            <v-alert v-if="passwordError" type="error" variant="tonal" density="compact" class="mt-4 mb-4">
              {{ passwordError }}
            </v-alert>
            <v-alert v-else-if="passwordSaved" type="success" variant="tonal" density="compact" class="mt-4 mb-4">
              {{ t('profile.security.saved') }}
            </v-alert>

            <v-btn
              variant="outlined"
              :class="{ 'mt-4': !passwordError && !passwordSaved }"
              :loading="savingPassword"
              @click="changePassword"
            >
              {{ t('profile.security.save') }}
            </v-btn>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-main>
</template>
