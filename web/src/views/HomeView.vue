<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'

const { t } = useI18n()
const auth = useAuthStore()

const userName = computed(() => auth.currentUser?.userName ?? '')
</script>

<template>
  <v-main>
    <v-container class="py-10">
      <h1 class="text-h4 font-weight-bold mb-2">{{ t('home.greeting', { name: userName }) }}</h1>
      <p class="text-body-1 text-medium-emphasis">{{ t('home.subtitle') }}</p>
    </v-container>
  </v-main>
</template>
