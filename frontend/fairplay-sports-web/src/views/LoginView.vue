<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/components/AuthLayout.vue'
import logoUrl from '@/assets/logo.png'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { t } = useI18n()

const justRegistered = computed(() => route.query.registered === '1')

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
    await router.push('/profile')
  } catch {
    submitError.value = t('login.failed')
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <AuthLayout>
    <template #brand>
      <img :src="logoUrl" :alt="t('common.appName')" class="auth-logo" />
    </template>

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

    <template #footer>
      {{ t('login.noAccount') }}
      <RouterLink to="/register" class="text-primary text-decoration-none font-weight-medium">
        {{ t('login.goRegister') }}
      </RouterLink>
    </template>
  </AuthLayout>
</template>

<style scoped>
.auth-logo {
  display: block;
  height: 200px;
  width: auto;
  max-width: 100%;
  /* Negative margins trim the dead space above/below the logo art so it
     sits closer to the card edge and to the first input. */
  margin: -24px auto -20px;
  object-fit: contain;
}
</style>
