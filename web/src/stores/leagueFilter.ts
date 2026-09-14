import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { AgeCategory, Division, FootballType } from '@/types/team'

/**
 * Modalidad/División/Categoría, shared between Standings and Teams so
 * switching between the two keeps looking at the same league slice instead
 * of resetting. Always has a value - a standings table needs one to mean
 * anything, and Teams reuses the same three for the same reason: browsing
 * "the teams in this league" only makes sense against one concrete league.
 */
export const useLeagueFilterStore = defineStore('leagueFilter', () => {
  const type = ref<FootballType>('Football11')
  const division = ref<Division>('Second')
  const category = ref<AgeCategory>('Infantiles')

  return { type, division, category }
})
