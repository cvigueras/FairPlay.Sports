<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { mdiCalendarMonthOutline, mdiSwordCross, mdiTranslate, mdiTrophyOutline } from '@mdi/js'
import { useAuthStore } from '@/stores/auth'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import logoUrl from '@/assets/logo.webp'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { t, locale } = useI18n()

const justRegistered = computed(() => route.query.registered === '1')

const heroFeatures = [
  { icon: mdiSwordCross, titleKey: 'login.feature1Title', bodyKey: 'login.feature1Body' },
  { icon: mdiCalendarMonthOutline, titleKey: 'login.feature2Title', bodyKey: 'login.feature2Body' },
  { icon: mdiTrophyOutline, titleKey: 'login.feature3Title', bodyKey: 'login.feature3Body' },
]

const form = reactive({
  email: '',
  password: '',
})

const errors = reactive({
  email: '',
  password: '',
})

const isSubmitting = ref(false)
const submitError = ref('')

function validate(): boolean {
  errors.email = !form.email
    ? t('validation.emailRequired')
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? t('validation.emailInvalid')
      : ''

  errors.password = !form.password ? t('validation.passwordRequired') : ''

  return !errors.email && !errors.password
}

async function handleSubmit() {
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await auth.login(form.email, form.password)
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
            <h2 class="text-h5 font-weight-bold">{{ t('login.title') }}</h2>
            <p class="text-body-2 text-medium-emphasis mt-1">{{ t('login.subtitle') }}</p>
          </div>

          <v-form novalidate @submit.prevent="handleSubmit">
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
              v-model="form.email"
              :label="t('register.email')"
              type="email"
              autocomplete="email"
              :placeholder="t('login.emailPlaceholder')"
              :error-messages="errors.email"
              class="mb-2"
            />

            <v-text-field
              v-model="form.password"
              :label="t('register.password')"
              type="password"
              autocomplete="current-password"
              :placeholder="t('common.passwordPlaceholder')"
              :error-messages="errors.password"
              class="mb-2"
            />

            <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ submitError }}
            </v-alert>

            <v-btn type="submit" block size="large" :loading="isSubmitting">
              {{ isSubmitting ? t('login.submitting') : t('login.submit') }}
            </v-btn>
          </v-form>

          <p class="login-panel__footer text-body-2 text-medium-emphasis">
            {{ t('login.noAccount') }}
            <RouterLink to="/register" class="text-primary text-decoration-none font-weight-medium">
              {{ t('login.goRegister') }}
            </RouterLink>
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
