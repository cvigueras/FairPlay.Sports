<script setup lang="ts">
import { computed } from 'vue'
import { teamsApi } from '@/lib/teams'
import type { Team } from '@/types/team'

const props = defineProps<{ team: Team; size?: number }>()

const dimension = computed(() => props.size ?? 96)

/** Up to two leading initials, for the fallback shield. */
const initials = computed(() =>
  props.team.name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0]?.toUpperCase() ?? '')
    .join(''),
)
</script>

<template>
  <v-img
    v-if="team.hasCrest"
    :src="teamsApi.crestUrl(team.id)"
    :alt="team.name"
    :width="dimension"
    :height="dimension"
    class="flex-grow-0"
  />

  <!-- Fallback (only if a crest fails to load): a shield in the brand colour. -->
  <svg
    v-else
    :width="dimension"
    :height="dimension"
    viewBox="0 0 100 100"
    role="img"
    :aria-label="team.name"
  >
    <path
      d="M50 6 L88 19 V49 C88 73 72 89 50 95 C28 89 12 73 12 49 V19 Z"
      fill="rgba(var(--v-theme-primary), 0.12)"
      stroke="rgb(var(--v-theme-primary))"
      stroke-width="3"
      stroke-linejoin="round"
    />
    <text
      x="50"
      y="55"
      text-anchor="middle"
      dominant-baseline="middle"
      font-size="30"
      font-weight="700"
      font-family="inherit"
      fill="rgb(var(--v-theme-primary))"
    >
      {{ initials }}
    </text>
  </svg>
</template>
