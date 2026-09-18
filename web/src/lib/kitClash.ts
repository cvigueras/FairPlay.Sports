import type { KitPattern, Team } from '@/types/team'
import type { TeamKitSlot } from '@/types/challenge'

export interface ResolvedKit {
  colorPrimary: string
  colorSecondary: string
  shortsColor: string
  kitPattern: KitPattern
  slot: TeamKitSlot
}

function firstKit(team: Team): ResolvedKit | null {
  if (!team.colorPrimary || !team.colorSecondary || !team.shortsColor || !team.kitPattern) return null
  return {
    colorPrimary: team.colorPrimary,
    colorSecondary: team.colorSecondary,
    shortsColor: team.shortsColor,
    kitPattern: team.kitPattern,
    slot: 'First',
  }
}

function secondKit(team: Team): ResolvedKit | null {
  if (
    !team.alternateColorPrimary ||
    !team.alternateColorSecondary ||
    !team.alternateShortsColor ||
    !team.alternateKitPattern
  ) {
    return null
  }
  return {
    colorPrimary: team.alternateColorPrimary,
    colorSecondary: team.alternateColorSecondary,
    shortsColor: team.alternateShortsColor,
    kitPattern: team.alternateKitPattern,
    slot: 'Second',
  }
}

/** The home team always wears its first kit. */
export function homeKit(homeTeam: Team): ResolvedKit | null {
  return firstKit(homeTeam)
}

/**
 * The away team's first kit, unless its primary colour matches the home team's - then its
 * second kit if that avoids the clash; null if neither does. A client-side preview of the
 * backend's own resolution (`SendChallengeHandler.ResolveAwayKitSlot`) - the server always
 * re-validates, this is only so the wizard can show the result before sending.
 */
export function resolveAwayKit(homeTeam: Team, awayTeam: Team): ResolvedKit | null {
  const home = firstKit(homeTeam)
  if (!home) return null

  const first = firstKit(awayTeam)
  if (first && first.colorPrimary.toLowerCase() !== home.colorPrimary.toLowerCase()) return first

  const second = secondKit(awayTeam)
  if (second && second.colorPrimary.toLowerCase() !== home.colorPrimary.toLowerCase()) return second

  return null
}
