<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useDisplay } from 'vuetify'
import {
  mdiAccountGroupOutline,
  mdiAccountOutline,
  mdiChevronRight,
  mdiEarth,
  mdiHomeOutline,
  mdiLogout,
  mdiMenu,
  mdiTrophyOutline,
} from '@mdi/js'
import { baseUrl } from '@/lib/http'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import { SUPPORTED_LOCALES, setLocale } from '@/plugins/i18n'
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import logoUrl from '@/assets/logo.webp'

const { t, locale } = useI18n()
const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const ui = useUiStore()
const { mobile } = useDisplay()

const userInitial = computed(() => auth.currentUser?.userName?.charAt(0).toUpperCase() ?? '')

const userPhotoUrl = computed(() => {
  const user = auth.currentUser
  if (!user?.hasPhoto) return null
  const bust = auth.photoVersion ? `?v=${auth.photoVersion}` : ''
  return `${baseUrl}/api/users/${user.id}/photo${bust}`
})

const memberSince = computed(() => {
  const user = auth.currentUser
  if (!user) return ''
  return new Date(user.createdAt).toLocaleDateString(locale.value, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

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
  if (route.name === 'team-detail') {
    return [
      { ...home, disabled: false },
      { title: t('nav.teams'), to: '/teams', disabled: false },
      { title: ui.breadcrumbLabel ?? '…', to: route.path, disabled: true },
    ]
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

    <v-breadcrumbs :items="breadcrumbs" density="compact" class="app-bar-crumbs">
      <template #divider>
        <v-icon :icon="mdiChevronRight" size="14" />
      </template>
    </v-breadcrumbs>

    <v-spacer />

    <template #append>
      <div class="app-bar-actions">
        <div v-if="auth.currentUser" class="app-bar-user d-none d-sm-flex">
          <v-menu :close-on-content-click="false" location="bottom end" offset="10">
            <template #activator="{ props }">
              <button
                type="button"
                class="app-bar-user__avatar-btn"
                :aria-label="t('profile.menu.open')"
                v-bind="props"
              >
                <span class="app-bar-user__avatar">
                  <img
                    v-if="userPhotoUrl"
                    :src="userPhotoUrl"
                    :alt="t('profile.photo.alt')"
                    class="app-bar-user__avatar-img"
                  />
                  <template v-else>{{ userInitial }}</template>
                </span>
              </button>
            </template>

            <v-card class="user-panel" flat>
              <div class="user-panel__avatar">
                <ProfileAvatar />
              </div>
              <p class="user-panel__name">{{ auth.currentUser.userName }}</p>
              <p class="user-panel__email">{{ auth.currentUser.email }}</p>
              <v-chip
                :color="auth.currentUser.role === 'Admin' ? 'amber-darken-2' : 'primary'"
                size="small"
                variant="tonal"
                class="mt-2"
              >
                {{ auth.currentUser.role }}
              </v-chip>
              <div class="user-panel__since">
                <span class="text-medium-emphasis">{{ t('profile.fields.memberSince') }}</span>
                <span class="font-weight-medium">{{ memberSince }}</span>
              </div>
            </v-card>
          </v-menu>

          <span class="app-bar-user__name">{{ t('profile.welcome', { name: auth.currentUser.userName }) }}</span>
        </div>

        <v-menu>
          <template #activator="{ props }">
            <button type="button" class="lang-pill" :aria-label="t('language.label')" v-bind="props">
              <v-icon :icon="mdiEarth" size="16" color="#64748b" />
              <span class="lang-pill__code">{{ locale.toUpperCase() }}</span>
            </button>
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
      </div>
    </template>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" :rail="rail">
    <div class="nav-brand" :class="{ 'nav-brand--rail': rail }">
      <img :src="logoUrl" :alt="t('common.appName')" class="nav-brand-logo" />
      <span v-if="!rail" class="nav-brand-name">{{ t('common.appName') }}</span>
    </div>
    <v-divider />

    <v-list nav density="comfortable" class="nav-list">
      <v-list-item
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        :prepend-icon="item.icon"
        :title="t(item.label)"
        rounded="lg"
      />
    </v-list>

    <template #append>
      <div class="pa-2">
        <v-btn
          v-if="rail"
          :icon="mdiLogout"
          variant="outlined"
          class="logout-btn"
          :loading="isLoggingOut"
          :aria-label="t('profile.logout')"
          @click="handleLogout"
        />
        <v-btn
          v-else
          block
          variant="outlined"
          :prepend-icon="mdiLogout"
          class="logout-btn"
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

.app-bar-crumbs :deep(.v-breadcrumbs-item) {
  color: #94a3b8;
  font-weight: 500;
  font-size: 13.5px;
}

.app-bar-crumbs :deep(.v-breadcrumbs-item--disabled) {
  color: #0f172a;
  font-weight: 700;
  opacity: 1;
}

/* Narrow screens: three levels ("Home / Teams / <team>") plus the language
   button don't fit at full size - shrink the crumbs and let a long trailing
   one (the team name) ellipsize instead of wrapping or overflowing. */
@media (max-width: 599px) {
  .app-bar-crumbs {
    font-size: 0.75rem;
    padding-inline: 0.25rem;
  }
  .app-bar-crumbs :deep(.v-breadcrumbs-item) {
    padding-inline: 2px;
  }
  .app-bar-crumbs :deep(.v-breadcrumbs-divider) {
    padding-inline: 2px;
  }
  .app-bar-crumbs :deep(.v-breadcrumbs-item--disabled) {
    display: inline-block;
    max-width: 32vw;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    vertical-align: middle;
  }
}

.app-bar-actions {
  display: flex;
  align-items: center;
  gap: 20px;
  padding-right: 4px;
}

.app-bar-user {
  align-items: center;
  gap: 10px;
}

.app-bar-user__avatar-btn {
  padding: 0;
  border: none;
  background: none;
  cursor: pointer;
  border-radius: 50%;
}

.app-bar-user__avatar {
  flex: 0 0 auto;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: #16a34a;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  overflow: hidden;
}

.app-bar-user__avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.user-panel {
  width: 280px;
  padding: 1.25rem 1.375rem;
  border-radius: 16px !important;
  border: 1px solid #e2e8f0;
  text-align: center;
}

.user-panel__avatar {
  display: flex;
  justify-content: center;
  margin-bottom: 0.75rem;
}

.user-panel__name {
  margin: 0;
  font-family: 'Space Grotesk', system-ui, sans-serif;
  font-weight: 700;
  color: #0f172a;
}

.user-panel__email {
  margin: 0.15rem 0 0;
  font-size: 0.8125rem;
  color: #64748b;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-panel__since {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  margin-top: 0.9rem;
  padding-top: 0.9rem;
  border-top: 1px solid #f1f5f9;
  font-size: 0.8125rem;
}

.app-bar-user__name {
  font-size: 14px;
  font-weight: 500;
  color: #334155;
}

.lang-pill {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 6px 6px 10px;
  border-radius: 999px;
  border: 1px solid #e2e8f0;
  background: #f8fafc;
  font: inherit;
  cursor: pointer;
}

.lang-pill__code {
  padding: 5px 10px;
  border-radius: 999px;
  background: #16a34a;
  color: #ffffff;
  font-size: 12px;
  font-weight: 700;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.9rem 1.1rem;
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
  font-family: 'Space Grotesk', system-ui, sans-serif;
  font-weight: 700;
  white-space: nowrap;
}

.nav-list {
  padding: 0.75rem;
}

.nav-list :deep(.v-list-item) {
  color: #475569;
  margin-bottom: 4px;
}

.nav-list :deep(.v-list-item .v-icon) {
  color: #475569;
}

.nav-list :deep(.v-list-item--active) {
  background: rgba(22, 163, 74, 0.1);
}

.nav-list :deep(.v-list-item--active),
.nav-list :deep(.v-list-item--active .v-icon),
.nav-list :deep(.v-list-item--active .v-list-item-title) {
  color: #15803d;
  font-weight: 700;
}

.logout-btn {
  border-radius: 10px;
  border-color: #e2e8f0 !important;
  color: #334155;
}
</style>
