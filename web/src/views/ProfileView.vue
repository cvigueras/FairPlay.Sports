<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mdiLockOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const { t, locale } = useI18n()

const user = computed(() => auth.currentUser)

const formatLongDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'long', day: 'numeric' })

const memberSince = computed(() => (user.value ? formatLongDate(user.value.createdAt) : ''))

/* ---- Personal info (username/email) ------------------------------------------ */

const accountForm = reactive({ userName: '', email: '' })
const accountErrors = reactive({ userName: '', email: '' })
const savingAccount = ref(false)
const accountSaved = ref(false)
const accountError = ref('')

watch(
  user,
  (value) => {
    if (!value) return
    accountForm.userName = value.userName
    accountForm.email = value.email
  },
  { immediate: true },
)

const accountDirty = computed(
  () => !!user.value && (accountForm.userName !== user.value.userName || accountForm.email !== user.value.email),
)

function validateAccount(): boolean {
  accountErrors.userName = !accountForm.userName.trim() ? t('validation.userNameRequired') : ''
  accountErrors.email = !accountForm.email
    ? t('validation.emailRequired')
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(accountForm.email)
      ? t('validation.emailInvalid')
      : ''

  return !accountErrors.userName && !accountErrors.email
}

async function saveAccount() {
  accountError.value = ''
  accountSaved.value = false
  if (!validateAccount()) return

  savingAccount.value = true
  try {
    await auth.updateProfile({ userName: accountForm.userName.trim(), email: accountForm.email.trim() })
    accountSaved.value = true
  } catch (error) {
    accountError.value = error instanceof ApiError ? error.message : t('profile.account.saveFailed')
  } finally {
    savingAccount.value = false
  }
}

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
      <!-- Personal info: avatar, editable username/email, role and member-since. -->
      <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5">
        <div class="d-flex flex-column flex-sm-row ga-6">
          <div class="d-flex flex-column align-center flex-shrink-0 ga-2">
            <ProfileAvatar />
            <p class="text-caption text-medium-emphasis mb-0">{{ t('profile.photo.change') }}</p>
          </div>

          <div class="flex-grow-1" style="min-width: 0">
            <v-row dense>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="accountForm.userName"
                  :label="t('profile.account.userName')"
                  :error-messages="accountErrors.userName"
                  hide-details="auto"
                />
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="accountForm.email"
                  :label="t('profile.account.email')"
                  :error-messages="accountErrors.email"
                  hide-details="auto"
                />
              </v-col>
            </v-row>

            <div class="d-flex align-center ga-3 flex-wrap mt-4 mb-4">
              <v-chip
                :color="user.role === 'Admin' ? 'amber-darken-2' : 'primary'"
                size="small"
                variant="tonal"
              >
                {{ user.role }}
              </v-chip>
              <span class="text-body-2 text-medium-emphasis">
                {{ t('profile.fields.memberSince') }} {{ memberSince }}
              </span>
            </div>

            <v-alert v-if="accountError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ accountError }}
            </v-alert>
            <v-alert v-else-if="accountSaved" type="success" variant="tonal" density="compact" class="mb-4">
              {{ t('profile.account.saved') }}
            </v-alert>

            <v-btn :loading="savingAccount" :disabled="!accountDirty" @click="saveAccount">
              {{ t('profile.account.save') }}
            </v-btn>
          </div>
        </div>
      </v-card>

      <!-- Security: change password. -->
      <v-card border flat rounded="xl" class="pa-6 pa-md-8 mb-5">
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

        <v-btn variant="outlined" :class="{ 'mt-4': !passwordError && !passwordSaved }" :loading="savingPassword" @click="changePassword">
          {{ t('profile.security.save') }}
        </v-btn>
      </v-card>
    </v-container>
  </v-main>
</template>
