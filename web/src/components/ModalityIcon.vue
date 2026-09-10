<script setup lang="ts">
import { computed } from 'vue'
import { MODALITY_COLOR } from '@/lib/modality'
import type { FootballType } from '@/types/team'

const props = withDefaults(defineProps<{ type: FootballType; size?: number; label?: string }>(), {
  size: 24,
})

const color = computed(() => MODALITY_COLOR[props.type])
const digit = computed(() => (props.type === 'Football11' ? '11' : '8'))

const FONT = "system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif"
</script>

<template>
  <svg
    :width="size"
    :height="size"
    viewBox="0 0 24 24"
    role="img"
    :aria-label="label ?? type"
    :style="{ color, flex: '0 0 auto' }"
    fill="none"
    stroke="currentColor"
    stroke-width="1.6"
    stroke-linecap="round"
    stroke-linejoin="round"
  >
    <!-- 11-a-side / 8-a-side: outdoor pitch framed by the shirt number -->
    <template v-if="type === 'Football11' || type === 'Football8'">
      <rect x="3" y="5" width="18" height="14" rx="2.4" />
      <line x1="12" y1="5" x2="12" y2="19" stroke-width="1" opacity="0.3" />
      <text
        x="12"
        y="12.4"
        text-anchor="middle"
        dominant-baseline="central"
        :font-size="type === 'Football11' ? 8.5 : 11"
        font-weight="800"
        :font-family="FONT"
        fill="currentColor"
        stroke="none"
      >{{ digit }}</text>
    </template>

    <!-- Futsal: rounded indoor court with a solid ball -->
    <template v-else-if="type === 'Futsal'">
      <rect x="2.5" y="5" width="19" height="14" rx="4.5" />
      <line x1="12" y1="5" x2="12" y2="19" stroke-width="1" opacity="0.3" />
      <circle cx="12" cy="12" r="3.9" fill="currentColor" stroke="none" />
      <path d="M12 8.7 13.7 11 12 12.2 10.3 11 Z" fill="#fff" stroke="none" opacity="0.9" />
      <circle cx="12" cy="14.2" r="0.85" fill="#fff" stroke="none" opacity="0.9" />
    </template>

    <!-- Beach soccer: ball and sun over the sand -->
    <template v-else>
      <circle cx="11.3" cy="8.3" r="3.7" />
      <path d="M11.3 5.2 12.7 7.9 10.2 9.2 Z" fill="currentColor" stroke="none" opacity="0.85" />
      <circle cx="19.2" cy="5" r="1.5" fill="currentColor" stroke="none" />
      <path d="M2.5 15.6 q2.4 -2 4.8 0 t4.8 0 t4.8 0 t4.8 0" />
      <path d="M2.5 19 q2.4 -2 4.8 0 t4.8 0 t4.8 0 t4.8 0" opacity="0.45" />
    </template>
  </svg>
</template>
