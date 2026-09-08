<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ApiError } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/components/AuthLayout.vue'

const router = useRouter()
const auth = useAuthStore()
const { t } = useI18n()

const form = reactive({
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const errors = reactive({
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const isSubmitting = ref(false)
const submitError = ref('')

function validate(): boolean {
  errors.userName = !form.userName.trim() ? t('validation.userNameRequired') : ''

  errors.email = !form.email
    ? t('validation.emailRequired')
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? t('validation.emailInvalid')
      : ''

  errors.password = !form.password
    ? t('validation.passwordRequired')
    : form.password.length < 8
      ? t('validation.passwordMinLength')
      : ''

  errors.confirmPassword =
    form.confirmPassword !== form.password ? t('validation.passwordsMismatch') : ''

  return (
    !errors.userName &&
    !errors.email &&
    !errors.password &&
    !errors.confirmPassword
  )
}

async function handleSubmit() {
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await auth.register({
      userName: form.userName.trim(),
      email: form.email,
      password: form.password,
    })
  } catch (error) {
    submitError.value = error instanceof ApiError ? error.message : t('register.failed')
    return
  } finally {
    isSubmitting.value = false
  }

  // Navigate only after the account was created; a router rejection here must
  // not surface as a registration error.
  await router.push({ path: '/login', query: { registered: '1' } })
}
</script>

<template>
  <AuthLayout :title="t('register.title')" :subtitle="t('register.subtitle')">
    <v-form novalidate @submit.prevent="handleSubmit">
      <v-text-field
        v-model="form.userName"
        :label="t('register.userName')"
        autocomplete="username"
        :placeholder="t('register.userNamePlaceholder')"
        :error-messages="errors.userName"
        class="mb-2"
      />

      <v-text-field
        v-model="form.email"
        :label="t('register.email')"
        type="email"
        autocomplete="email"
        :placeholder="t('register.emailPlaceholder')"
        :error-messages="errors.email"
        class="mb-2"
      />

      <v-text-field
        v-model="form.password"
        :label="t('register.password')"
        type="password"
        autocomplete="new-password"
        :placeholder="t('common.passwordPlaceholder')"
        :error-messages="errors.password"
        class="mb-2"
      />

      <v-text-field
        v-model="form.confirmPassword"
        :label="t('register.confirmPassword')"
        type="password"
        autocomplete="new-password"
        :placeholder="t('common.passwordPlaceholder')"
        :error-messages="errors.confirmPassword"
        class="mb-2"
      />

      <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
        {{ submitError }}
      </v-alert>

      <v-btn type="submit" block size="large" :loading="isSubmitting">
        {{ isSubmitting ? t('register.submitting') : t('register.submit') }}
      </v-btn>
    </v-form>

    <template #footer>
      {{ t('register.haveAccount') }}
      <RouterLink to="/login" class="text-primary text-decoration-none font-weight-medium">
        {{ t('register.goLogin') }}
      </RouterLink>
    </template>
  </AuthLayout>
</template>
