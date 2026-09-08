<script setup lang="ts">
defineProps<{
  /** Shown as the card heading, unless the `brand` slot is filled. */
  title?: string
  subtitle?: string
}>()
</script>

<template>
  <v-main>
    <div class="auth-bg d-flex align-center justify-center pa-6">
      <v-card
        theme="fairplayDark"
        class="auth-card pa-6 pa-sm-8 w-100"
        max-width="420"
        rounded="xl"
        flat
      >
        <div class="text-center mb-6">
          <slot name="brand">
            <h1 class="text-h5 font-weight-bold">{{ title }}</h1>
          </slot>
          <p v-if="subtitle" class="text-body-2 text-medium-emphasis mt-1">{{ subtitle }}</p>
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
.auth-card {
  background: rgba(15, 23, 42, 0.28);
  backdrop-filter: blur(18px) saturate(140%);
  -webkit-backdrop-filter: blur(18px) saturate(140%);
  border: 1px solid rgba(255, 255, 255, 0.14);
  box-shadow: 0 24px 60px rgba(0, 0, 0, 0.5);
}
</style>
