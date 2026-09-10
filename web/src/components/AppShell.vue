<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
const route = useRoute()
const auth = useAuthStore()
const { mobile } = useDisplay()

// Routes are flat, so every page is just "Home / <page>".
const CRUMB_LABELS: Record<string, string> = {
  home: 'nav.home',
  profile: 'nav.profile',
  teams: 'nav.teams',
  standings: 'nav.standings',
}

const breadcrumbs = computed(() => {
  const home = { title: t('nav.home'), to: '/', disabled: route.name === 'home' }
  if (route.name === 'home' || !route.name) {
    return [home]
  }
  const key = CRUMB_LABELS[String(route.name)]
  return [
    { ...home, disabled: false },
    { title: key ? t(key) : String(route.name), to: route.path, disabled: true },
  ]
})

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

    <v-breadcrumbs :items="breadcrumbs" density="compact" class="app-bar-crumbs" />

    <v-spacer />

    <template #append>
      <span
        v-if="auth.currentUser"
        class="text-body-2 font-weight-bold mx-3 d-none d-sm-inline"
      >
        {{ t('profile.welcome', { name: auth.currentUser.userName }) }}
      </span>

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
      <div class="pa-2">
        <v-btn
          v-if="rail"
          :icon="mdiLogout"
          variant="outlined"
          :loading="isLoggingOut"
          :aria-label="t('profile.logout')"
          @click="handleLogout"
        />
        <v-btn
          v-else
          block
          variant="outlined"
          :prepend-icon="mdiLogout"
          :loading="isLoggingOut"
          @click="handleLogout"
        >
          {{ isLoggingOut ? t('profile.loggingOut') : t('profile.logout') }}
        </v-btn>
      </div>
    </template>
  </v-navigation-drawer>

  <slot />
</template>

<style scoped>
.app-bar-crumbs {
  padding-inline: 0.5rem;
  min-width: 0;
}

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
