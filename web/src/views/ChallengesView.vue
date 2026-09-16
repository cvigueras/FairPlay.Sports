<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <v-main>
    <v-container class="py-10">
      <p class="text-body-1">{{ t('challenges.placeholder') }}</p>
    </v-container>
  </v-main>
</template>
