<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

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
    ? 'El email es obligatorio.'
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? 'Introduce un email válido.'
      : ''

  errors.password = !form.password
    ? 'La contraseña es obligatoria.'
    : form.password.length < 6
      ? 'Debe tener al menos 6 caracteres.'
      : ''

  return !errors.email && !errors.password
}

async function handleSubmit() {
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await auth.login(form.email)
    await router.push('/dashboard')
  } catch {
    submitError.value = 'No se ha podido iniciar sesión. Inténtalo de nuevo.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <form class="auth-card" novalidate @submit.prevent="handleSubmit">
      <h1 class="auth-title">Iniciar sesión</h1>
      <p class="auth-subtitle">Bienvenido de nuevo a FairPlay Sports</p>

      <label class="field">
        <span class="field-label">Email</span>
        <input
          v-model="form.email"
          type="email"
          autocomplete="email"
          placeholder="tu@email.com"
          :class="{ invalid: errors.email }"
        />
        <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
      </label>

      <label class="field">
        <span class="field-label">Contraseña</span>
        <input
          v-model="form.password"
          type="password"
          autocomplete="current-password"
          placeholder="••••••••"
          :class="{ invalid: errors.password }"
        />
        <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
      </label>

      <p v-if="submitError" class="form-error">{{ submitError }}</p>

      <button class="submit-button" type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? 'Entrando...' : 'Entrar' }}
      </button>

      <p class="auth-footer">
        ¿No tienes cuenta?
        <RouterLink to="/register">Regístrate</RouterLink>
      </p>
    </form>
  </div>
</template>
