<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

async function handleLogout() {
  auth.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1 class="auth-title">¡Hola, {{ auth.currentUser?.name }}!</h1>
      <p class="auth-subtitle">
        Has iniciado sesión como <strong>{{ auth.currentUser?.email }}</strong> (sesión dummy, sin
        backend real).
      </p>
      <button class="submit-button" type="button" @click="handleLogout">Cerrar sesión</button>
    </div>
  </div>
</template>
