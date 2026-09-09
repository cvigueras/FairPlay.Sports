<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAccountGroupOutline,
  mdiAccountOutline,
  mdiHomeOutline,
  mdiLogout,
  mdiMenu,
  mdiTranslate,
  mdiTrophyOutline,
} from '@mdi/js'
import { useAuthStore } from '@/stores/auth'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import logoUrl from '@/assets/logo.webp'

const { t, locale } = useI18n()
const router = useRouter()
const auth = useAuthStore()
const { mobile } = useDisplay()

const RAIL_STORAGE_KEY = 'fps_nav_rail'

function initialRail(): boolean {
  try {
    return localStorage.getItem(RAIL_STORAGE_KEY) === '1'
  } catch {
    // localStorage may be unavailable (private mode, blocked cookies).
    return false
  }
}

// `drawer` opens/closes the overlay drawer on mobile; `rail` collapses it to an
// icon strip on desktop. Default is expanded; the rail choice is remembered.
const drawer = ref(true)
const rail = ref(initialRail())

function toggleNav(): void {
  if (mobile.value) {
    drawer.value = !drawer.value
    return
  }
  rail.value = !rail.value
  try {
    localStorage.setItem(RAIL_STORAGE_KEY, rail.value ? '1' : '0')
  } catch {
    // Persisting is best-effort; the toggle still applies for this session.
  }
}

const navItems = [
  { to: '/', icon: mdiHomeOutline, label: 'nav.home' },
  { to: '/profile', icon: mdiAccountOutline, label: 'nav.profile' },
  { to: '/teams', icon: mdiAccountGroupOutline, label: 'nav.teams' },
  { to: '/standings', icon: mdiTrophyOutline, label: 'nav.standings' },
]

const isLoggingOut = ref(false)

async function handleLogout(): Promise<void> {
  isLoggingOut.value = true
  try {
    await auth.logout()
  } finally {
    isLoggingOut.value = false
  }
  router.push('/login')
}
</script>

<template>
  <v-app-bar flat border="b">
    <template #prepend>
      <v-app-bar-nav-icon :icon="mdiMenu" @click="toggleNav" />
    </template>

    <v-spacer />

    <template #append>
      <v-menu>
        <template #activator="{ props }">
          <v-btn v-bind="props" :prepend-icon="mdiTranslate" variant="text">
            {{ t(`language.${locale}`) }}
          </v-btn>
        </template>
        <v-list>
          <v-list-item
            v-for="option in SUPPORTED_LOCALES"
            :key="option"
            :active="option === locale"
            @click="setLocale(option)"
          >
            <v-list-item-title>{{ t(`language.${option}`) }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </template>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" :rail="rail">
    <v-divider />
    <div class="nav-brand" :class="{ 'nav-brand--rail': rail }">
      <img :src="logoUrl" :alt="t('common.appName')" class="nav-brand-logo" />
      <span v-if="!rail" class="nav-brand-name">{{ t('common.appName') }}</span>
    </div>
    <v-divider />

    <v-list nav density="comfortable">
      <v-list-item
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        :prepend-icon="item.icon"
        :title="t(item.label)"
      />
    </v-list>

    <template #append>
      <v-list nav density="comfortable">
        <v-list-item
          :prepend-icon="mdiLogout"
          :title="t('profile.logout')"
          :disabled="isLoggingOut"
          @click="handleLogout"
        />
      </v-list>
    </template>
  </v-navigation-drawer>

  <slot />
</template>

<style scoped>
.nav-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 0.9rem;
  /* Very, very subtle blue. */
  background: rgba(59, 130, 246, 0.04);
}

.nav-brand--rail {
  padding-inline: 0;
  justify-content: center;
}

.nav-brand-logo {
  flex: 0 0 auto;
  height: 36px;
  width: auto;
}

.nav-brand-name {
  font-weight: 600;
  white-space: nowrap;
}
</style>
