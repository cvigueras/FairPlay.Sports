<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const errors = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const isSubmitting = ref(false)
const submitError = ref('')

function validate(): boolean {
  errors.name = !form.name.trim() ? 'El nombre es obligatorio.' : ''

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

  errors.confirmPassword =
    form.confirmPassword !== form.password ? 'Las contraseñas no coinciden.' : ''

  return !errors.name && !errors.email && !errors.password && !errors.confirmPassword
}

async function handleSubmit() {
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await auth.register(form.name, form.email)
    await router.push('/dashboard')
  } catch {
    submitError.value = 'No se ha podido completar el registro. Inténtalo de nuevo.'
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
        <span class="field-label">Nombre</span>
        <input
          v-model="form.name"
          type="text"
          autocomplete="name"
          placeholder="Tu nombre"
          :class="{ invalid: errors.name }"
        />
        <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
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
