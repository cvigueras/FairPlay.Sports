<script setup lang="ts">
import KitSwatch from '@/components/KitSwatch.vue'
import type { KitPattern } from '@/types/team'

withDefaults(
  defineProps<{ pattern: KitPattern; primary: string; secondary: string; shorts: string; size?: number }>(),
  { size: 96 },
)
</script>

<template>
  <div class="fp-kit-preview" :style="{ width: `${size}px` }">
    <KitSwatch :pattern="pattern" :primary="primary" :secondary="secondary" :size="size" />
    <svg :width="size * 0.8" :height="size * 0.48" viewBox="0 0 100 60" aria-hidden="true" class="fp-kit-preview-shorts">
      <rect x="8" y="0" width="84" height="20" rx="10" :fill="shorts" stroke="rgba(15, 23, 42, 0.32)" stroke-width="1.6" />
      <path
        d="M10,14 L44,14 L44,46 C44,52 39,57 33,57 L21,57 C15,57 10,52 10,46 Z"
        :fill="shorts"
        stroke="rgba(15, 23, 42, 0.32)"
        stroke-width="1.6"
      />
      <path
        d="M56,14 L90,14 L90,46 C90,52 85,57 79,57 L67,57 C61,57 56,52 56,46 Z"
        :fill="shorts"
        stroke="rgba(15, 23, 42, 0.32)"
        stroke-width="1.6"
      />
    </svg>
  </div>
</template>

<style scoped>
.fp-kit-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.fp-kit-preview-shorts {
  margin-top: -2px;
}
</style>
