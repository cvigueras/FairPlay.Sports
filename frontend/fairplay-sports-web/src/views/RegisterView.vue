<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({
  userName: '',
  email: '',
  team: '',
  password: '',
  confirmPassword: '',
})

const errors = reactive({
  userName: '',
  email: '',
  team: '',
  password: '',
  confirmPassword: '',
})

const isSubmitting = ref(false)
const submitError = ref('')

function validate(): boolean {
  errors.userName = !form.userName.trim() ? 'El nombre de usuario es obligatorio.' : ''

  errors.email = !form.email
    ? 'El email es obligatorio.'
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? 'Introduce un email válido.'
      : ''

  errors.team = !form.team.trim() ? 'El equipo es obligatorio.' : ''

  errors.password = !form.password
    ? 'La contraseña es obligatoria.'
    : form.password.length < 8
      ? 'Debe tener al menos 8 caracteres.'
      : ''

  errors.confirmPassword =
    form.confirmPassword !== form.password ? 'Las contraseñas no coinciden.' : ''

  return (
    !errors.userName &&
    !errors.email &&
    !errors.team &&
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
      team: form.team.trim(),
      password: form.password,
    })
    await router.push({ path: '/login', query: { registered: '1' } })
  } catch (error) {
    submitError.value =
      error instanceof ApiError
        ? error.message
        : 'No se ha podido completar el registro. Inténtalo de nuevo.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <form class="auth-card" novalidate @submit.prevent="handleSubmit">
      <h1 class="auth-title">Crear cuenta</h1>
      <p class="auth-subtitle">Únete a FairPlay Sports</p>

      <label class="field">
        <span class="field-label">Nombre de usuario</span>
        <input
          v-model="form.userName"
          type="text"
          autocomplete="username"
          placeholder="tu_usuario"
          :class="{ invalid: errors.userName }"
        />
        <span v-if="errors.userName" class="field-error">{{ errors.userName }}</span>
      </label>

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
        <span class="field-label">Equipo</span>
        <input
          v-model="form.team"
          type="text"
          autocomplete="organization"
          placeholder="Tu equipo"
          :class="{ invalid: errors.team }"
        />
        <span v-if="errors.team" class="field-error">{{ errors.team }}</span>
      </label>

      <label class="field">
        <span class="field-label">Contraseña</span>
        <input
          v-model="form.password"
          type="password"
          autocomplete="new-password"
          placeholder="••••••••"
          :class="{ invalid: errors.password }"
        />
        <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
      </label>

      <label class="field">
        <span class="field-label">Confirmar contraseña</span>
        <input
          v-model="form.confirmPassword"
          type="password"
          autocomplete="new-password"
          placeholder="••••••••"
          :class="{ invalid: errors.confirmPassword }"
        />
        <span v-if="errors.confirmPassword" class="field-error">{{ errors.confirmPassword }}</span>
      </label>

      <p v-if="submitError" class="form-error">{{ submitError }}</p>

      <button class="submit-button" type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? 'Creando cuenta...' : 'Registrarme' }}
      </button>

      <p class="auth-footer">
        ¿Ya tienes cuenta?
        <RouterLink to="/login">Inicia sesión</RouterLink>
      </p>
    </form>
  </div>
</template>
