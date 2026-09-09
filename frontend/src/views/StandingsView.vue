<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <v-main>
    <v-container class="py-10">
      <h1 class="text-h4 font-weight-bold mb-2">{{ t('nav.standings') }}</h1>
      <p class="text-body-1 text-medium-emphasis">{{ t('common.comingSoon') }}</p>
    </v-container>
  </v-main>
</template>
