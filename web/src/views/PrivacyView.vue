<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { mdiArrowLeft } from '@mdi/js'

// Keep in sync with the backend's PrivacyPolicy.CurrentVersion: that is the
// version recorded on the user when they accept at sign-up.
const POLICY_VERSION = '2026-09-30'

const { t, tm, rt } = useI18n()
const router = useRouter()

type Message = Parameters<typeof rt>[0]

// tm() hands back compiled messages, not strings: each one has to go through
// rt(). Inside a computed so the text follows a language switch.
const sections = computed(() =>
  (tm('privacy.sections') as { h: Message; p: Message }[]).map((section) => ({
    h: rt(section.h),
    p: rt(section.p),
  })),
)

function goBack() {
  if (window.history.length > 1) router.back()
  else router.push({ name: 'register' })
}
</script>

<template>
  <v-main>
    <v-container class="privacy py-6 py-md-10">
      <v-btn variant="text" :prepend-icon="mdiArrowLeft" class="mb-4" @click="goBack">
        {{ t('privacy.back') }}
      </v-btn>

      <h1 class="text-h4 font-weight-bold">{{ t('privacy.title') }}</h1>
      <p class="text-body-2 text-medium-emphasis mb-6">
        {{ t('privacy.version', { version: POLICY_VERSION }) }}
      </p>

      <section v-for="section in sections" :key="section.h" class="mb-5">
        <h2 class="text-h6 font-weight-bold mb-1">{{ section.h }}</h2>
        <p class="text-body-1">{{ section.p }}</p>
      </section>
    </v-container>
  </v-main>
</template>

<style scoped>
.privacy {
  max-width: 760px;
}
</style>
