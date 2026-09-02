<script setup lang="ts">
defineProps<{
  title: string
  subtitle: string
}>()
</script>

<template>
  <v-main>
    <div class="auth-bg d-flex align-center justify-center pa-6">
      <v-card class="pa-6 pa-sm-8 w-100" max-width="420" rounded="xl" elevation="8">
        <div class="text-center mb-6">
          <h1 class="text-h5 font-weight-bold">{{ title }}</h1>
          <p class="text-body-2 text-medium-emphasis mt-1">{{ subtitle }}</p>
        </div>

        <slot />

        <p v-if="$slots.footer" class="text-center text-body-2 text-medium-emphasis mt-5">
          <slot name="footer" />
        </p>
      </v-card>
    </div>
  </v-main>
</template>

<style scoped>
/* Own the full viewport height explicitly: Vuetify's `fill-height` (height: 100%)
   collapses here because <v-main>'s height comes from flex-stretch, not a
   definite value, so a percentage height has nothing to resolve against. */
.auth-bg {
  min-height: 100dvh;
  background: linear-gradient(160deg, #0f172a 0%, #1e293b 100%);
}
</style>
