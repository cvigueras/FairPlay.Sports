<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { mdiCircle, mdiLogout } from '@mdi/js'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const user = computed(() => auth.currentUser)
const isLoggingOut = ref(false)

const initials = computed(() =>
  (user.value?.userName ?? '')
    .split(/[\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join(''),
)

const memberSince = computed(() => {
  if (!user.value) return ''
  return new Date(user.value.createdAt).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

const details = computed(() => {
  if (!user.value) return []
  return [
    { label: 'Usuario', value: user.value.userName },
    { label: 'Email', value: user.value.email },
    { label: 'Equipo', value: user.value.team },
    { label: 'Rol', value: user.value.role },
    { label: 'Miembro desde', value: memberSince.value },
    { label: 'Estado', value: user.value.active ? 'Activo' : 'Inactivo' },
  ]
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
  <v-app-bar flat border="b" color="surface">
    <v-app-bar-title>
      <span class="d-inline-flex align-center ga-2 font-weight-bold">
        <v-icon :icon="mdiCircle" color="primary" size="12" />
        FairPlay Sports
      </span>
    </v-app-bar-title>

    <template #append>
      <span v-if="user" class="text-body-2 text-medium-emphasis mr-3 d-none d-sm-inline">
        {{ user.userName }}
      </span>
      <v-btn
        variant="outlined"
        :prepend-icon="mdiLogout"
        :loading="isLoggingOut"
        @click="handleLogout"
      >
        Cerrar sesión
      </v-btn>
    </template>
  </v-app-bar>

  <v-main>
    <v-container v-if="user" class="py-10" style="max-width: 800px">
      <h1 class="text-h5 font-weight-bold">Mi perfil</h1>
      <p class="text-body-2 text-medium-emphasis mb-6">Tu información en FairPlay Sports</p>

      <v-card border flat rounded="xl" class="mb-6 pa-6 d-flex align-center ga-4">
        <v-avatar color="primary" size="64" class="text-h6 font-weight-bold">
          {{ initials }}
        </v-avatar>
        <div class="flex-grow-1" style="min-width: 0">
          <p class="text-h6 font-weight-bold text-truncate">{{ user.userName }}</p>
          <p class="text-body-2 text-medium-emphasis text-truncate">{{ user.email }}</p>
          <v-chip
            :color="user.role === 'Admin' ? 'amber-darken-2' : 'primary'"
            size="small"
            variant="tonal"
            class="mt-1"
          >
            {{ user.role }}
          </v-chip>
        </div>
      </v-card>

      <v-card border flat rounded="xl">
        <v-list>
          <template v-for="(row, index) in details" :key="row.label">
            <v-divider v-if="index > 0" />
            <v-list-item class="py-3">
              <template #subtitle>
                <span class="text-caption text-uppercase">{{ row.label }}</span>
              </template>
              <v-list-item-title class="font-weight-medium">{{ row.value }}</v-list-item-title>
            </v-list-item>
          </template>
        </v-list>
      </v-card>
    </v-container>
  </v-main>
</template>
