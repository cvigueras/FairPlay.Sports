<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { mdiCalendarMonthOutline, mdiSwordCross, mdiTranslate, mdiTrophyOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import logoUrl from '@/assets/logo.webp'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { t, locale } = useI18n()

// Login and register share this one screen (hero + panel); switching between
// them is a local state flip, not a route change, so the hero never remounts.
const mode = ref<'login' | 'register'>(route.name === 'register' ? 'register' : 'login')
const justRegistered = ref(route.query.registered === '1')

const heroFeatures = [
  { icon: mdiSwordCross, titleKey: 'login.feature1Title', bodyKey: 'login.feature1Body' },
  { icon: mdiCalendarMonthOutline, titleKey: 'login.feature2Title', bodyKey: 'login.feature2Body' },
  { icon: mdiTrophyOutline, titleKey: 'login.feature3Title', bodyKey: 'login.feature3Body' },
]

const isSubmitting = ref(false)
const submitError = ref('')

function switchMode(next: 'login' | 'register') {
  mode.value = next
  submitError.value = ''
  loginErrors.email = ''
  loginErrors.password = ''
  registerErrors.userName = ''
  registerErrors.email = ''
  registerErrors.password = ''
  registerErrors.confirmPassword = ''
}

const loginForm = reactive({
  email: '',
  password: '',
})

const loginErrors = reactive({
  email: '',
  password: '',
})

function validateLogin(): boolean {
  loginErrors.email = !loginForm.email
    ? t('validation.emailRequired')
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(loginForm.email)
      ? t('validation.emailInvalid')
      : ''

  loginErrors.password = !loginForm.password ? t('validation.passwordRequired') : ''

  return !loginErrors.email && !loginErrors.password
}

async function handleLoginSubmit() {
  submitError.value = ''
  if (!validateLogin()) return

  isSubmitting.value = true
  try {
    await auth.login(loginForm.email, loginForm.password)
  } catch {
    submitError.value = t('login.failed')
    return
  } finally {
    isSubmitting.value = false
  }

  // Navigate only after a successful sign-in; a router rejection here must not
  // surface as a "wrong credentials" message.
  await router.push('/profile')
}

const registerForm = reactive({
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const registerErrors = reactive({
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
})

function validateRegister(): boolean {
  registerErrors.userName = !registerForm.userName.trim() ? t('validation.userNameRequired') : ''

  registerErrors.email = !registerForm.email
    ? t('validation.emailRequired')
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(registerForm.email)
      ? t('validation.emailInvalid')
      : ''

  registerErrors.password = !registerForm.password
    ? t('validation.passwordRequired')
    : registerForm.password.length < 8
      ? t('validation.passwordMinLength')
      : ''

  registerErrors.confirmPassword =
    registerForm.confirmPassword !== registerForm.password ? t('validation.passwordsMismatch') : ''

  return (
    !registerErrors.userName &&
    !registerErrors.email &&
    !registerErrors.password &&
    !registerErrors.confirmPassword
  )
}

async function handleRegisterSubmit() {
  submitError.value = ''
  if (!validateRegister()) return

  isSubmitting.value = true
  try {
    await auth.register({
      userName: registerForm.userName.trim(),
      email: registerForm.email,
      password: registerForm.password,
    })
  } catch (error) {
    submitError.value = error instanceof ApiError ? error.message : t('register.failed')
    return
  } finally {
    isSubmitting.value = false
  }

  // The account was created but the user still needs to sign in; switch back
  // to the login form in place rather than navigating.
  loginForm.email = registerForm.email
  justRegistered.value = true
  switchMode('login')
}
</script>

<template>
  <v-main>
    <div class="login-shell">
      <section class="login-hero">
        <div class="login-hero__brand">
          <img :src="logoUrl" :alt="t('common.appName')" class="login-hero__logo" />
          <span class="login-hero__brand-name">{{ t('common.appName') }}</span>
        </div>

        <div class="login-hero__pitch">
          <span class="login-hero__eyebrow">{{ t('login.heroEyebrow') }}</span>
          <h1 class="login-hero__title">{{ t('login.heroTitle') }}</h1>
          <p class="login-hero__description">{{ t('login.heroDescription') }}</p>
        </div>

        <ul class="login-hero__features">
          <li v-for="feature in heroFeatures" :key="feature.titleKey">
            <span class="login-hero__feature-icon">
              <v-icon :icon="feature.icon" size="22" color="#4ade80" />
            </span>
            <span class="login-hero__feature-text">
              <strong>{{ t(feature.titleKey) }}</strong>
              <span>{{ t(feature.bodyKey) }}</span>
            </span>
          </li>
        </ul>
      </section>

      <section class="login-panel">
        <v-menu>
          <template #activator="{ props }">
            <v-btn
              class="login-panel__lang"
              variant="text"
              size="small"
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

        <div class="login-panel__form">
          <div class="login-panel__heading">
            <h2 class="text-h5 font-weight-bold">
              {{ mode === 'login' ? t('login.title') : t('register.title') }}
            </h2>
            <p class="text-body-2 text-medium-emphasis mt-1">
              {{ mode === 'login' ? t('login.subtitle') : t('register.subtitle') }}
            </p>
          </div>

          <v-form v-if="mode === 'login'" novalidate @submit.prevent="handleLoginSubmit">
            <v-alert
              v-if="justRegistered"
              type="success"
              variant="tonal"
              density="compact"
              class="mb-4"
            >
              {{ t('login.justRegistered') }}
            </v-alert>

            <v-text-field
              v-model="loginForm.email"
              :label="t('register.email')"
              type="email"
              autocomplete="email"
              :placeholder="t('login.emailPlaceholder')"
              :error-messages="loginErrors.email"
              class="mb-2"
            />

            <v-text-field
              v-model="loginForm.password"
              :label="t('register.password')"
              type="password"
              autocomplete="current-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error-messages="loginErrors.password"
              class="mb-2"
            />

            <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ submitError }}
            </v-alert>

            <v-btn type="submit" block size="large" :loading="isSubmitting">
              {{ isSubmitting ? t('login.submitting') : t('login.submit') }}
            </v-btn>
          </v-form>

          <v-form v-else novalidate @submit.prevent="handleRegisterSubmit">
            <v-text-field
              v-model="registerForm.userName"
              :label="t('register.userName')"
              autocomplete="username"
              :placeholder="t('register.userNamePlaceholder')"
              :error-messages="registerErrors.userName"
              class="mb-2"
            />

            <v-text-field
              v-model="registerForm.email"
              :label="t('register.email')"
              type="email"
              autocomplete="email"
              :placeholder="t('register.emailPlaceholder')"
              :error-messages="registerErrors.email"
              class="mb-2"
            />

            <v-text-field
              v-model="registerForm.password"
              :label="t('register.password')"
              type="password"
              autocomplete="new-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error-messages="registerErrors.password"
              class="mb-2"
            />

            <v-text-field
              v-model="registerForm.confirmPassword"
              :label="t('register.confirmPassword')"
              type="password"
              autocomplete="new-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error-messages="registerErrors.confirmPassword"
              class="mb-2"
            />

            <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ submitError }}
            </v-alert>

            <v-btn type="submit" block size="large" :loading="isSubmitting">
              {{ isSubmitting ? t('register.submitting') : t('register.submit') }}
            </v-btn>
          </v-form>

          <p class="login-panel__footer text-body-2 text-medium-emphasis">
            <template v-if="mode === 'login'">
              {{ t('login.noAccount') }}
              <button
                type="button"
                class="login-panel__switch text-primary font-weight-medium"
                @click="switchMode('register')"
              >
                {{ t('login.goRegister') }}
              </button>
            </template>
            <template v-else>
              {{ t('register.haveAccount') }}
              <button
                type="button"
                class="login-panel__switch text-primary font-weight-medium"
                @click="switchMode('login')"
              >
                {{ t('register.goLogin') }}
              </button>
            </template>
          </p>
        </div>
      </section>
    </div>
  </v-main>
</template>

<style scoped>
.login-shell {
  min-height: 100dvh;
  display: flex;
}

.login-hero {
  flex: 1 1 62%;
  max-width: 900px;
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 40px;
  padding: 56px 64px;
  color: #f8fafc;
  background-image:
    linear-gradient(160deg, rgba(15, 23, 42, 0.88) 10%, rgba(15, 23, 42, 0.55) 55%, rgba(15, 23, 42, 0.86) 100%),
    url('../assets/login-bg.webp');
  background-size: cover;
  background-position: center;
}

.login-hero__brand {
  display: flex;
  align-items: center;
  gap: 14px;
}

.login-hero__logo {
  width: 60px;
  height: 60px;
  object-fit: contain;
}

.login-hero__brand-name {
  font-size: 1.25rem;
  font-weight: 700;
  letter-spacing: 0.2px;
}

.login-hero__pitch {
  display: flex;
  flex-direction: column;
  gap: 20px;
  max-width: 480px;
}

.login-hero__eyebrow {
  display: inline-flex;
  width: fit-content;
  padding: 6px 14px;
  border-radius: 999px;
  background: rgba(52, 211, 153, 0.16);
  border: 1px solid rgba(52, 211, 153, 0.4);
  color: #6ee7b7;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 1px;
  text-transform: uppercase;
}

.login-hero__title {
  margin: 0;
  font-size: clamp(1.75rem, 1.2rem + 2vw, 2.75rem);
  line-height: 1.15;
  font-weight: 800;
  letter-spacing: -0.5px;
}

.login-hero__description {
  margin: 0;
  font-size: 1.0625rem;
  line-height: 1.6;
  color: #cbd5e1;
}

.login-hero__features {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.login-hero__features li {
  display: flex;
  align-items: flex-start;
  gap: 16px;
}

.login-hero__feature-icon {
  flex: 0 0 44px;
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.14);
  display: flex;
  align-items: center;
  justify-content: center;
}

.login-hero__feature-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 0.85rem;
  color: #94a3b8;
  line-height: 1.45;
}

.login-hero__feature-text strong {
  font-size: 0.95rem;
  color: #f8fafc;
}

.login-panel {
  position: relative;
  flex: 1 1 38%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  background: #f8fafc;
}

.login-panel__lang {
  position: absolute;
  top: 24px;
  right: 24px;
}

.login-panel__form {
  width: 100%;
  max-width: 380px;
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.login-panel__footer {
  margin: 0;
  text-align: center;
}

.login-panel__switch {
  background: none;
  border: none;
  padding: 0;
  font: inherit;
  cursor: pointer;
  text-decoration: none;
}

@media (max-width: 899px) {
  .login-shell {
    flex-direction: column;
  }

  .login-hero {
    max-width: none;
    padding: 32px 24px 36px;
    gap: 28px;
  }

  .login-hero__title {
    font-size: 1.75rem;
  }

  .login-panel {
    padding: 40px 24px 48px;
  }
}
</style>
