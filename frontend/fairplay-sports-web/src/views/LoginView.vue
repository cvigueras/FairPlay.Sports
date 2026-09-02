<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/components/AuthLayout.vue'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

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
    ? 'El email es obligatorio.'
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)
      ? 'Introduce un email válido.'
      : ''

  errors.password = !form.password ? 'La contraseña es obligatoria.' : ''

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
    submitError.value = 'Email o contraseña incorrectos.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <AuthLayout title="Iniciar sesión" subtitle="Bienvenido de nuevo a FairPlay Sports">
    <v-form novalidate @submit.prevent="handleSubmit">
      <v-alert
        v-if="justRegistered"
        type="success"
        variant="tonal"
        density="compact"
        class="mb-4"
      >
        Cuenta creada. Inicia sesión para continuar.
      </v-alert>

      <v-text-field
        v-model="form.email"
        label="Email"
        type="email"
        autocomplete="email"
        :error-messages="errors.email"
        class="mb-2"
      />

      <v-text-field
        v-model="form.password"
        label="Contraseña"
        type="password"
        autocomplete="current-password"
        :error-messages="errors.password"
        class="mb-2"
      />

      <v-alert v-if="submitError" type="error" variant="tonal" density="compact" class="mb-4">
        {{ submitError }}
      </v-alert>

      <v-btn type="submit" block size="large" :loading="isSubmitting">
        {{ isSubmitting ? 'Entrando...' : 'Entrar' }}
      </v-btn>
    </v-form>

    <template #footer>
      ¿No tienes cuenta?
      <RouterLink to="/register" class="text-primary text-decoration-none font-weight-medium">
        Regístrate
      </RouterLink>
    </template>
  </AuthLayout>
</template>
