<script setup lang="ts">
import { computed, useId } from 'vue'
import type { KitPattern } from '@/types/team'

const props = withDefaults(
  defineProps<{ pattern: KitPattern; primary: string; secondary: string; size?: number }>(),
  { size: 40 },
)

/** A stylised jersey silhouette (collar, tapered sleeves, rounded hem) drawn as
 *  one closed path in a 0-100 box - the neckline is carved into the outline
 *  itself (the dip between the shoulders), so no separate collar cutout is
 *  needed. Reused both as the clip region for the pattern fills below and,
 *  unclipped, as the outline on top so the silhouette edge stays crisp
 *  regardless of what's clipped inside it. */
const JERSEY_PATH =
  'M 6,24 C 2,28 2,34 5,38 L 24,52 L 24,86 C 24,91 28,95 33,95 L 67,95 C 72,95 76,91 76,86 L 76,52 L 95,38 C 98,34 98,28 94,24 L 75,9 C 73,8 70,9 69,11 C 65,19 58,23 50,23 C 42,23 35,19 31,11 C 30,9 27,8 25,9 Z'

const clipId = `kit-swatch-clip-${useId()}`
const fadeId = `kit-swatch-fade-${useId()}`

const checkerCells = computed(() => {
  const cells: { x: number; y: number }[] = []
  for (let row = 0; row < 4; row++) {
    for (let col = 0; col < 4; col++) {
      if ((row + col) % 2 === 1) cells.push({ x: 10 + col * 20, y: 13 + row * 21 })
    }
  }
  return cells
})
</script>

<template>
  <svg :width="size" :height="size" viewBox="0 0 100 100" aria-hidden="true">
    <defs>
      <clipPath :id="clipId">
        <path :d="JERSEY_PATH" />
      </clipPath>
      <linearGradient :id="fadeId" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="100" y2="0">
        <stop offset="0" :stop-color="primary" />
        <stop offset="1" :stop-color="secondary" />
      </linearGradient>
    </defs>
    <g :clip-path="`url(#${clipId})`">
      <rect x="0" y="0" width="100" height="100" :fill="pattern === 'Fade' ? `url(#${fadeId})` : primary" />

      <template v-if="pattern === 'Stripes'">
        <rect v-for="i in [0, 1, 2]" :key="i" :x="30 + i * 18" y="0" width="7" height="100" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Hoops'">
        <rect v-for="i in [0, 1, 2]" :key="i" x="0" :y="45 + i * 17" width="100" height="8" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'FullHoops'">
        <!-- Same idea as Hoops but spans collar to hem, no solid band on top. -->
        <rect v-for="i in [0, 1, 2, 3, 4]" :key="i" x="0" :y="12 + i * 18" width="100" height="8" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Halves'">
        <rect x="50" y="0" width="50" height="100" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Sash'">
        <rect x="-20" y="40" width="160" height="22" transform="rotate(35 50 50)" :fill="secondary" />
      </template>

      <template v-else-if="pattern === 'Checkered'">
        <rect
          v-for="(cell, i) in checkerCells"
          :key="i"
          :x="cell.x"
          :y="cell.y"
          width="20"
          height="21"
          :fill="secondary"
        />
      </template>

      <template v-else-if="pattern === 'Sleeves'">
        <!-- Covers both sleeve regions only - stays clear of the torso. -->
        <rect x="0" y="13" width="25" height="42" :fill="secondary" />
        <rect x="75" y="13" width="25" height="42" :fill="secondary" />
      </template>
    </g>

    <path :d="JERSEY_PATH" fill="none" stroke="rgba(15, 23, 42, 0.32)" stroke-width="1.6" />
  </svg>
</template>
