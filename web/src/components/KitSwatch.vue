<script setup lang="ts">
import { computed, useId } from 'vue'
import type { KitPattern } from '@/types/team'

const props = withDefaults(
  defineProps<{ pattern: KitPattern; primary: string; secondary: string; size?: number }>(),
  { size: 40 },
)

/** mdiTshirtCrew's path (viewBox 0 0 24 24) - reused both as the clip region
 *  for the pattern fills below and, unclipped, as the outline on top so the
 *  silhouette edge stays crisp regardless of what's clipped inside it. */
const JERSEY_PATH =
  'M16,21H8A1,1 0 0,1 7,20V12.07L5.7,13.07C5.31,13.46 4.68,13.46 4.29,13.07L1.46,10.29C1.07,9.9 1.07,9.27 1.46,8.88L7.34,3H9C9,4.1 10.34,5 12,5C13.66,5 15,4.1 15,3H16.66L22.54,8.88C22.93,9.27 22.93,9.9 22.54,10.29L19.71,13.12C19.32,13.5 18.69,13.5 18.3,13.12L17,12.12V20A1,1 0 0,1 16,21'

const clipId = `kit-swatch-clip-${useId()}`
const fadeId = `kit-swatch-fade-${useId()}`

const checkerCells = computed(() => {
  const cells: { x: number; y: number }[] = []
  for (let row = 0; row < 4; row++) {
    for (let col = 0; col < 4; col++) {
      if ((row + col) % 2 === 1) cells.push({ x: col * 6, y: row * 6 })
    }
  }
  return cells
})
</script>

<template>
  <svg :width="size" :height="size" viewBox="0 0 24 24" aria-hidden="true">
    <defs>
      <clipPath :id="clipId">
        <path :d="JERSEY_PATH" />
      </clipPath>
      <linearGradient :id="fadeId" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="24" y2="0">
        <stop offset="0" :stop-color="primary" />
        <stop offset="1" :stop-color="secondary" />
      </linearGradient>
    </defs>
    <g :clip-path="`url(#${clipId})`">
      <rect x="0" y="0" width="24" height="24" :fill="pattern === 'Fade' ? `url(#${fadeId})` : primary" />

      <template v-if="pattern === 'Stripes'">
        <rect v-for="i in [0, 1, 2]" :key="i" :x="4 + i * 8" y="0" width="4" height="24" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Hoops'">
        <!-- Confined to the torso band (y 11-22) - the jersey silhouette's
             collar/sleeves above it are too narrow for a hoop to read there. -->
        <rect v-for="i in [0, 1, 2]" :key="i" x="0" :y="11.5 + i * 3.6" width="24" height="2" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'FullHoops'">
        <!-- Same idea as Hoops but spans collar to hem, no solid band on top. -->
        <rect v-for="i in [0, 1, 2, 3, 4]" :key="i" x="0" :y="3 + i * 3.6" width="24" height="2" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Halves'">
        <rect x="12" y="0" width="12" height="24" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Sash'">
        <rect x="-6" y="9" width="36" height="6" :fill="secondary" transform="rotate(35 12 12)" />
      </template>

      <template v-else-if="pattern === 'Checkered'">
        <rect
          v-for="(cell, i) in checkerCells"
          :key="i"
          :x="cell.x"
          :y="cell.y"
          width="6"
          height="6"
          :fill="secondary"
        />
      </template>

      <template v-else-if="pattern === 'Sleeves'">
        <!-- Covers both sleeve triangles (silhouette clips each to shape). -->
        <rect x="0" y="3" width="7" height="11" :fill="secondary" />
        <rect x="17" y="3" width="7" height="11" :fill="secondary" />
      </template>
    </g>

    <path :d="JERSEY_PATH" fill="none" stroke="rgba(15, 23, 42, 0.25)" stroke-width="0.6" />
  </svg>
</template>
