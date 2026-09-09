<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppShell from '@/components/AppShell.vue'

// Guest-only screens (login, register) keep their own full-screen layout with
// no app chrome; every other route renders inside the nav shell.
const route = useRoute()
const chromeless = computed(() => route.meta.guestOnly === true)
</script>

<template>
  <v-app>
    <RouterView v-if="chromeless" />
    <AppShell v-else>
      <RouterView />
    </AppShell>
  </v-app>
</template>
