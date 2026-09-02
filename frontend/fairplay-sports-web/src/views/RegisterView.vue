<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/components/AuthLayout.vue'

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
  <AuthLayout title="Crear cuenta" subtitle="Únete a FairPlay Sports">
    <v-form novalidate @submit.prevent="handleSubmit">
      <v-text-field
        v-model="form.userName"
        label="Nombre de usuario"
        autocomplete="username"
        :error-messages="errors.userName"
        class="mb-2"
      />

      <v-text-field
        v-model="form.email"
        label="Email"
        type="email"
        autocomplete="email"
        :error-messages="errors.email"
        class="mb-2"
      />

      <v-text-field
        v-model="form.team"
        label="Equipo"
        autocomplete="organization"
        :error-messages="errors.team"
        class="mb-2"
      />

      <v-text-field
        v-model="form.password"
        label="Contraseña"
        type="password"
        autocomplete="new-password"
        :error-messages="errors.password"
        class="mb-2"
      />

      <v-text-field
        v-model="form.confirmPassword"
        label="Confirmar contraseña"
        type="password"
        autocomplete="new-password"
        :error-messages="errors.confirmPassword"
        class="mb-2"
      />

      <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
        {{ submitError }}
      </v-alert>

      <v-btn type="submit" block size="large" :loading="isSubmitting">
        {{ isSubmitting ? 'Creando cuenta...' : 'Registrarme' }}
      </v-btn>
    </v-form>

    <template #footer>
      ¿Ya tienes cuenta?
      <RouterLink to="/login" class="text-primary text-decoration-none font-weight-medium">
        Inicia sesión
      </RouterLink>
    </template>
  </AuthLayout>
</template>
