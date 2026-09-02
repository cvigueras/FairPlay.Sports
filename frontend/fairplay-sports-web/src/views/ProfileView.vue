<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const user = computed(() => auth.currentUser)
const isLoggingOut = ref(false)

const memberSince = computed(() => {
  if (!user.value) return ''
  return new Date(user.value.createdAt).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

async function handleLogout() {
  isLoggingOut.value = true
  try {
    await auth.logout()
    await router.push('/login')
  } finally {
    isLoggingOut.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card" v-if="user">
      <h1 class="auth-title">¡Hola, {{ user.userName }}!</h1>
      <p class="auth-subtitle">Este es tu perfil de FairPlay Sports</p>

      <dl class="profile-details">
        <div class="profile-row">
          <dt>Usuario</dt>
          <dd>{{ user.userName }}</dd>
        </div>
        <div class="profile-row">
          <dt>Email</dt>
          <dd>{{ user.email }}</dd>
        </div>
        <div class="profile-row">
          <dt>Equipo</dt>
          <dd>{{ user.team }}</dd>
        </div>
        <div class="profile-row">
          <dt>Rol</dt>
          <dd>{{ user.role }}</dd>
        </div>
        <div class="profile-row">
          <dt>Miembro desde</dt>
          <dd>{{ memberSince }}</dd>
        </div>
      </dl>

      <button
        class="submit-button"
        type="button"
        :disabled="isLoggingOut"
        @click="handleLogout"
      >
        {{ isLoggingOut ? 'Cerrando sesión...' : 'Cerrar sesión' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.profile-details {
  margin: 1rem 0 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.profile-row {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.95rem;
}

.profile-row dt {
  color: #64748b;
  font-weight: 500;
}

.profile-row dd {
  margin: 0;
  color: #0f172a;
  font-weight: 600;
  text-align: right;
  word-break: break-word;
}
</style>
