<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppShell from '@/components/AppShell.vue'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

// Guest-only screens (login, register) keep their own full-screen layout with
// no app chrome; so do public pages (privacy policy) while nobody is signed in,
// since the nav shell only makes sense for an authenticated user. Every other
// route renders inside the nav shell.
const route = useRoute()
const auth = useAuthStore()
const chromeless = computed(
  () => route.meta.guestOnly === true || (route.meta.public === true && !auth.isAuthenticated),
)

const ui = useUiStore()
</script>

<template>
  <v-app>
    <RouterView v-if="chromeless" />
    <AppShell v-else>
      <RouterView />
    </AppShell>

    <!-- App-wide toast for transient success/error/warning results - see
         stores/ui.ts. Views call ui.notify(...) instead of rendering their
         own inline banner. -->
    <v-snackbar v-model="ui.toastShow" :color="ui.toastColor" location="bottom right" :timeout="5000">
      {{ ui.toastMessage }}
    </v-snackbar>
  </v-app>
</template>
