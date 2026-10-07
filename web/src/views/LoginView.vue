<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import { mdiCalendarMonthOutline, mdiSwordCross, mdiTranslate, mdiTrophyOutline } from '@mdi/js'
import { ApiError } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'
import { TEAM_MEMBER_ROLES, type TeamMemberRole } from '@/types/team'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import logoUrl from '@/assets/logo.webp'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { t, locale } = useI18n()
const { mobile } = useDisplay()

// Mobile redesigns the panel as a translucent "glass" card over the hero
// photo (see .login-panel below) instead of desktop's solid light panel
// beside it - the fairplayDark theme (vuetify.ts) was already defined for
// exactly this, just unused until now. Applying it through v-theme-provider
// recolors every Vuetify control inside (inputs, buttons, alerts) for free;
// on desktop this resolves to the app's own default theme, a no-op.
const panelTheme = computed(() => (mobile.value ? 'fairplayDark' : 'fairplay'))

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
  registerErrors.firstName = ''
  registerErrors.lastName = ''
  registerErrors.primaryRole = ''
  registerErrors.email = ''
  registerErrors.password = ''
  registerErrors.confirmPassword = ''
  registerErrors.privacy = ''
  registerErrors.age = ''
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
  await router.push('/')
}

const roleItems = computed(() =>
  TEAM_MEMBER_ROLES.map((role) => ({ value: role, title: t(`profile.team.memberRoles.${role}`) })),
)

const registerForm = reactive({
  primaryRole: null as TeamMemberRole | null,
  firstName: '',
  lastName: '',
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
  acceptedPrivacy: false,
  confirmedAge: false,
})

const registerErrors = reactive({
  primaryRole: '',
  firstName: '',
  lastName: '',
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
  privacy: '',
  age: '',
})

// An empty required register field (and an unticked required check) is flagged
// red without a message; only the role and format errors spell out what is wrong.
const REQUIRED = 'required'

function shown(error: string): string {
  return error === REQUIRED ? '' : error
}

function validateRegister(): boolean {
  registerErrors.primaryRole = !registerForm.primaryRole ? t('validation.primaryRoleRequired') : ''
  registerErrors.firstName = !registerForm.firstName.trim() ? REQUIRED : ''
  registerErrors.lastName = !registerForm.lastName.trim() ? REQUIRED : ''
  registerErrors.userName = !registerForm.userName.trim() ? REQUIRED : ''

  registerErrors.email = !registerForm.email
    ? REQUIRED
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(registerForm.email)
      ? t('validation.emailInvalid')
      : ''

  registerErrors.password = !registerForm.password
    ? REQUIRED
    : registerForm.password.length < 8
      ? t('validation.passwordMinLength')
      : ''

  registerErrors.confirmPassword = !registerForm.confirmPassword
    ? REQUIRED
    : registerForm.confirmPassword !== registerForm.password
      ? t('validation.passwordsMismatch')
      : ''

  registerErrors.privacy = !registerForm.acceptedPrivacy ? REQUIRED : ''

  registerErrors.age = !registerForm.confirmedAge ? REQUIRED : ''

  return (
    !registerErrors.primaryRole &&
    !registerErrors.firstName &&
    !registerErrors.lastName &&
    !registerErrors.userName &&
    !registerErrors.email &&
    !registerErrors.password &&
    !registerErrors.confirmPassword &&
    !registerErrors.privacy &&
    !registerErrors.age
  )
}

async function handleRegisterSubmit() {
  submitError.value = ''
  if (!validateRegister()) return

  isSubmitting.value = true
  try {
    await auth.register({
      userName: registerForm.userName.trim(),
      firstName: registerForm.firstName.trim(),
      lastName: registerForm.lastName.trim(),
      email: registerForm.email,
      password: registerForm.password,
      primaryRole: registerForm.primaryRole!,
      acceptedPrivacyPolicy: registerForm.acceptedPrivacy,
      confirmedMinimumAge: registerForm.confirmedAge,
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

          <!-- Mobile only: the language switcher moves up next to the brand
               name, over the photo - desktop keeps its own instance where it
               already was, top-right of the light panel below. -->
          <v-menu v-if="mobile">
            <template #activator="{ props }">
              <v-btn
                class="login-hero__lang"
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
        </div>

        <div class="login-hero__pitch">
          <span class="login-hero__eyebrow">{{ t('login.heroEyebrow') }}</span>
          <h1 v-if="!mobile" class="login-hero__title">{{ t('login.heroTitle') }}</h1>
          <p v-if="!mobile" class="login-hero__description">{{ t('login.heroDescription') }}</p>
        </div>

        <ul v-if="!mobile || mode === 'login'" class="login-hero__features">
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

      <section class="login-panel" :class="{ 'login-panel--register': mode === 'register' }">
        <v-theme-provider :theme="panelTheme">
        <v-menu v-if="!mobile">
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
            <h2>{{ mode === 'login' ? t('login.subtitle') : t('register.subtitle') }}</h2>
            <hr class="login-panel__rule" />
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
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-text-field
              v-model="loginForm.password"
              :label="t('register.password')"
              type="password"
              autocomplete="current-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error-messages="loginErrors.password"
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ submitError }}
            </v-alert>

            <v-btn type="submit" block size="large" class="login-panel__submit" :loading="isSubmitting">
              {{ isSubmitting ? t('login.submitting') : t('login.submit') }}
            </v-btn>
          </v-form>

          <v-form v-else novalidate @submit.prevent="handleRegisterSubmit">
            <div class="login-panel__group">{{ t('register.aboutYou') }}</div>

            <div class="login-panel__role">
              <span id="register-role-label" class="login-panel__role-label">
                {{ t('register.primaryRole') }}
              </span>
              <div role="group" aria-labelledby="register-role-label" class="login-panel__pills">
                <button
                  v-for="role in roleItems"
                  :key="role.value"
                  type="button"
                  class="login-panel__pill"
                  :class="{ 'login-panel__pill--selected': registerForm.primaryRole === role.value }"
                  :aria-pressed="registerForm.primaryRole === role.value"
                  @click="registerForm.primaryRole = role.value"
                >
                  {{ role.title }}
                </button>
              </div>
              <span v-if="registerErrors.primaryRole" class="fp-error">
                {{ registerErrors.primaryRole }}
              </span>
            </div>

            <div class="login-panel__names">
              <v-text-field
                v-model="registerForm.firstName"
                :label="t('register.firstName')"
                autocomplete="given-name"
                :error="!!registerErrors.firstName"
              :error-messages="shown(registerErrors.firstName)"
                density="compact"
                hide-details="auto"
                class="login-panel__field"
              />

              <v-text-field
                v-model="registerForm.lastName"
                :label="t('register.lastName')"
                autocomplete="family-name"
                :error="!!registerErrors.lastName"
              :error-messages="shown(registerErrors.lastName)"
                density="compact"
                hide-details="auto"
                class="login-panel__field"
              />
            </div>

            <div class="login-panel__group">{{ t('register.yourAccount') }}</div>

            <v-text-field
              v-model="registerForm.userName"
              :label="t('register.userName')"
              autocomplete="username"
              :placeholder="t('register.userNamePlaceholder')"
              :error="!!registerErrors.userName"
              :error-messages="shown(registerErrors.userName)"
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-text-field
              v-model="registerForm.email"
              :label="t('register.email')"
              type="email"
              autocomplete="email"
              :placeholder="t('register.emailPlaceholder')"
              :error="!!registerErrors.email"
              :error-messages="shown(registerErrors.email)"
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-text-field
              v-model="registerForm.password"
              :label="t('register.password')"
              type="password"
              autocomplete="new-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error="!!registerErrors.password"
              :error-messages="shown(registerErrors.password)"
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-text-field
              v-model="registerForm.confirmPassword"
              :label="t('register.confirmPassword')"
              type="password"
              autocomplete="new-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error="!!registerErrors.confirmPassword"
              :error-messages="shown(registerErrors.confirmPassword)"
              density="compact"
              hide-details="auto"
              class="login-panel__field"
            />

            <v-checkbox
              v-model="registerForm.confirmedAge"
              :label="t('register.ageConfirm')"
              density="compact"
              hide-details="auto"
              :error="!!registerErrors.age"
              class="login-panel__check"
            />

            <v-checkbox
              v-model="registerForm.acceptedPrivacy"
              density="compact"
              hide-details="auto"
              :error="!!registerErrors.privacy"
              class="login-panel__check mb-2"
            >
              <template #label>
                <span>
                  {{ t('register.privacyPrefix') }}
                  <router-link
                    :to="{ name: 'privacy' }"
                    target="_blank"
                    class="text-primary"
                    @click.stop
                  >
                    {{ t('register.privacyLink') }}
                  </router-link>
                </span>
              </template>
            </v-checkbox>

            <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ submitError }}
            </v-alert>

            <v-btn type="submit" block size="large" class="login-panel__submit" :loading="isSubmitting">
              {{ isSubmitting ? t('register.submitting') : t('register.submit') }}
            </v-btn>
          </v-form>

          <p class="login-panel__footer text-body-2 text-medium-emphasis">
            <template v-if="mode === 'login'">
              <strong>{{ t('login.noAccount') }}</strong> {{ ' ' }}
              <button
                type="button"
                class="login-panel__switch text-primary font-weight-medium"
                @click="switchMode('register')"
              >
                {{ t('login.goRegister') }}
              </button>
            </template>
            <template v-else>
              <strong>{{ t('register.haveAccount') }}</strong> {{ ' ' }}
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
        </v-theme-provider>
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
  padding: 48px 24px 96px;
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

/* Pinned to the panel's bottom so it sits at the same height in login and
   register, whatever the height of the (vertically centred) form above it. */
.login-panel__footer {
  position: absolute;
  right: 24px;
  bottom: 40px;
  left: 24px;
  margin: 0;
  text-align: center;
}

.login-panel__group {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 4px 0 12px;
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 1px;
  text-transform: uppercase;
  color: #64748b;
}

.login-panel__group::after {
  content: '';
  flex: 1;
  height: 1px;
  background: #e2e8f0;
}

.login-panel__heading h2 {
  margin: 0;
  text-wrap: balance;
  font-size: 1.375rem;
  font-weight: 700;
  line-height: 1.25;
  color: #0f172a;
}

/* Short accent rule under the heading, same indigo as the submit button. */
.login-panel__rule {
  width: 48px;
  height: 3px;
  margin: 14px 0 0;
  border: 0;
  border-radius: 2px;
  opacity: 1;
  background: rgb(var(--v-theme-primary));
}

.login-panel__switch {
  background: none;
  border: none;
  padding: 0;
  font: inherit;
  cursor: pointer;
  text-decoration: none;
}

.login-panel__submit {
  height: 52px;
  font-size: 0.9375rem;
  font-weight: 600;
}

.login-panel__role {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 18px;
}

.login-panel__role-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #334155;
}

.login-panel__pills {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.login-panel__pill {
  height: 38px;
  padding: 0 16px;
  border-radius: 999px;
  border: 1.5px solid #cbd5e1;
  background: #ffffff;
  color: #334155;
  font: inherit;
  font-size: 0.8125rem;
  font-weight: 600;
  cursor: pointer;
}

.login-panel__pill--selected {
  background: rgb(var(--v-theme-primary));
  border-color: rgb(var(--v-theme-primary));
  color: #ffffff;
}

.login-panel__names {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 12px;
}

.login-panel__check :deep(.v-selection-control) {
  min-height: 32px;
  --v-selection-control-size: 32px;
}

.login-panel__form :deep(.v-checkbox .v-label) {
  font-size: 0.84rem;
  opacity: 1;
}

/* hide-details="auto" collapses the reserved message row until a field has an
   error, so the (longer) register form stays compact; this restores the gap. */
.login-panel__field {
  --v-input-control-height: 48px;
  margin-bottom: 12px;
}

/* Mobile: no separate panel at all - the photo fills the whole screen and
   both the hero's own content (brand, pitch, features) and the panel's
   (form, footer) stack directly on top of it via CSS Grid, sharing the
   same cell instead of sitting in two flex rows one above the other. The
   hero's content anchors to the top, the panel's to the bottom (where the
   photo's own gradient is darkest). The headline and description are
   desktop-only depth (see the v-if next to each in the template); the
   feature list on top of that only shows for login - register keeps just
   the eyebrow, since its four-field form needs more of the screen than
   login's two. .login-shell is pinned to the viewport height (not
   min-height) so the page itself never scrolls; .login-panel keeps
   overflow-y: auto as a fallback in case a short viewport still can't fit
   everything. */
@media (max-width: 899px) {
  .login-shell {
    display: grid;
    /* Without an explicit, shrinkable track the implicit row is auto-sized to
       the tallest item: a taller register form grows the row past 100dvh,
       .login-panel's height: 100% then follows it, so it never overflows (and
       never scrolls) and the clipped bottom - the Register button - is
       unreachable. minmax(0, 1fr) pins the row to the viewport instead. */
    grid-template-columns: minmax(0, 1fr);
    grid-template-rows: minmax(0, 1fr);
    height: 100dvh;
    overflow: hidden;
  }

  .login-hero {
    grid-area: 1 / 1;
    max-width: none;
    height: 100%;
    justify-content: flex-start;
    padding: 18px 22px 20px;
    gap: 24px;
  }

  .login-hero__pitch {
    gap: 18px;
  }

  .login-hero__features {
    gap: 12px;
    /* The hero's own gap shrank by 20px to lift the eyebrow; keep the feature
       list (login only) where it was. */
    margin-top: 20px;
  }

  .login-hero__features li {
    gap: 10px;
  }

  .login-hero__feature-icon {
    flex: 0 0 32px;
    width: 32px;
    height: 32px;
    border-radius: 9px;
  }

  .login-hero__feature-icon :deep(.v-icon) {
    font-size: 16px;
  }

  .login-hero__feature-text {
    font-size: 0.75rem;
  }

  .login-hero__feature-text strong {
    font-size: 0.8125rem;
  }

  .login-panel {
    grid-area: 1 / 1;
    height: 100%;
    min-height: 0;
    /* Desktop leaves this as the default row (align-items/justify-content
       center the form both ways next to the hero). Never overridden until
       now, it silently stayed row here too, so "center"/"flex-end" below
       were centering vertically and pushing horizontally - not the bottom-
       anchored, horizontally-centered stack this layout actually wants -
       which is what was making the form's left/right margins uneven. */
    flex-direction: column;
    align-items: center;
    /* Top-anchored with the form pushed down via margin-top: auto (below)
       rather than justify-content: flex-end: with flex-end, content taller
       than the panel (short iPhone viewport with Safari's toolbars) overflows
       upward where it can't be scrolled to, and the footer's "Regístrate"
       button is lost. This way the overflow runs downward and scrolls. */
    justify-content: flex-start;
    /* Clear the iPhone home indicator / Safari bottom bar. */
    padding: 0 22px calc(32px + env(safe-area-inset-bottom, 0px));
    background: none;
    color: #ffffff;
    /* Confirmed both login and register fit with zero overflow (measured),
       so this is a fallback for an unusually short viewport, not something
       that should normally engage - scrollbar-gutter: stable would reserve
       its width unconditionally either way, which was actually the cause
       of an uneven left/right margin (measured: 22px left, 32px right)
       when nothing was even scrolling. */
    overflow-y: auto;
    /* .login-hero and .login-panel share the same grid cell above, each at
       the full cell height - .login-panel paints on top (later in the DOM),
       so even though its content hugs the bottom (justify-content:
       flex-end), its own box still covers the empty space above that
       content, over .login-hero's brand row. That silently ate every click
       aimed at .login-hero__lang there, so the button looked dead. Passing
       clicks through the empty area and re-enabling them on .login-panel's
       actual children fixes it without otherwise changing the layout.
       v-theme-provider itself renders no wrapper element here (no
       withBackground prop), so its direct children - the mobile v-menu (not
       rendered; desktop-only), .login-panel__form and .login-panel__footer -
       are the real, styleable targets. */
    pointer-events: none;
  }

  .login-panel > * {
    pointer-events: auto;
  }

  /* The register form can grow with its error messages: keep it below the
     hero's brand row and let the panel scroll, instead of anchoring it to the
     bottom and letting it climb over the logo. */
  .login-panel > .login-panel__form {
    margin-top: auto;
  }

  .login-panel--register {
    padding-top: 136px;
  }

  .login-panel--register .login-panel__field {
    margin-bottom: 12px;
  }

  .login-hero__brand {
    justify-content: space-between;
  }

  .login-hero__lang {
    color: #f8fafc;
    border: 1px solid rgba(255, 255, 255, 0.25);
    background: rgba(255, 255, 255, 0.08);
    border-radius: 8px;
  }

  .login-panel__form {
    width: 100%;
    max-width: 340px;
    gap: 12px;
  }

  /* "Iniciar sesión"/"Crear cuenta" and their subtitles are dropped
     entirely on mobile - the hero's own headline ("Reta a otros
     equipos...") already sits right above the form, so a second heading
     read as redundant labelling once everything shares the same photo. */
  .login-panel__heading {
    display: none;
  }

  /* Same text-medium-emphasis dimming as the heading's subtitle above,
     on "¿No tienes cuenta?"/"¿Ya tienes cuenta?". */
  .login-panel__footer {
    color: #ffffff;
    opacity: 1;
  }

  /* Same reasoning for the 8px gap Vuetify's mb-2 puts under every field -
     fine for login's two fields, adds up across register's four. */
  .login-panel__form :deep(.mb-2) {
    margin-bottom: 4px;
  }

  /* Register is one field per row here (role, names, account, passwords), so
     the group captions - a desktop nicety - go and the row gap tightens. */
  .login-panel__footer {
    position: static;
    margin-top: 16px;
    flex-shrink: 0;
  }

  .login-panel__switch {
    min-height: 44px;
    padding: 0 4px;
  }

  .login-panel__group {
    display: none;
  }

  .login-panel__role {
    margin-bottom: 14px;
  }

  .login-panel__role-label {
    color: rgba(255, 255, 255, 0.85);
  }

  .login-panel__pill {
    height: 40px;
    border-color: rgba(255, 255, 255, 0.28);
    background: rgba(255, 255, 255, 0.06);
    color: #e2e8f0;
  }

  .login-panel__pill--selected {
    background: #818cf8;
    border-color: #818cf8;
    color: #1e1b4b;
    font-weight: 700;
  }

  .login-panel__field {
    margin-bottom: 6px;
  }

  /* v-theme-provider (fairplayDark) already recolors every field's border,
     label and text for a dark background - this adds the subtle tinted
     fill the approved mockup gave each input, which an outlined field
     otherwise leaves fully transparent. */
  .login-panel :deep(.v-field) {
    background: rgba(255, 255, 255, 0.06);
  }

  /* fairplayDark's primary (#818CF8) is light enough that dark text reads
     better on it than Vuetify's own automatic on-primary pick - matches
     the approved mockup's button exactly instead of leaving it to chance. */
  .login-panel :deep(.v-btn) {
    color: #1e1b4b !important;
  }
}
</style>
