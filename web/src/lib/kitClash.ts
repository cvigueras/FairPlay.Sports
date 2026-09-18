import type { KitPattern, Team } from '@/types/team'
import type { TeamKitSlot } from '@/types/challenge'

export interface ResolvedKit {
  colorPrimary: string
  colorSecondary: string
  shortsColor: string
  kitPattern: KitPattern
  slot: TeamKitSlot
}

/** Both kits are known (or unknown) and, when both are known, whether they clash - purely
 *  informational, it never blocks sending or accepting a challenge. */
export interface KitInfo {
  home: ResolvedKit | null
  away: ResolvedKit | null
  clash: boolean
}

export function firstKit(team: Team): ResolvedKit | null {
  if (!team.colorPrimary || !team.colorSecondary || !team.shortsColor || !team.kitPattern) return null
  return {
    colorPrimary: team.colorPrimary,
    colorSecondary: team.colorSecondary,
    shortsColor: team.shortsColor,
    kitPattern: team.kitPattern,
    slot: 'First',
  }
}

export function secondKit(team: Team): ResolvedKit | null {
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

const sameColor = (a: string, b: string) => a.toLowerCase() === b.toLowerCase()

/** A team's kit in a given slot, or null if it hasn't configured that one. */
export function kitBySlot(team: Team, slot: TeamKitSlot): ResolvedKit | null {
  return slot === 'First' ? firstKit(team) : secondKit(team)
}

/** The home team always wears its first kit. Null if it hasn't configured one. */
export function homeKit(homeTeam: Team): ResolvedKit | null {
  return firstKit(homeTeam)
}

/**
 * The away team's first kit, unless its primary colour matches the home team's - then its
 * second kit if that avoids the clash; otherwise falls back to its first kit anyway (accepting
 * the clash) if it has one, else null (no kit configured at all). A client-side preview of the
 * backend's own resolution (`SendChallengeHandler.ResolveAwayKitSlot`) - the server is the
 * source of truth, this is only so the wizard can show the result before sending. Never returns
 * null just because of a clash - a clash is informational, never blocking.
 */
export function resolveAwayKit(homeTeam: Team, awayTeam: Team): ResolvedKit | null {
  const home = firstKit(homeTeam)
  const first = firstKit(awayTeam)

  if (!home) return first

  if (first && !sameColor(first.colorPrimary, home.colorPrimary)) return first

  const second = secondKit(awayTeam)
  if (second && !sameColor(second.colorPrimary, home.colorPrimary)) return second

  return first
}

export function resolveKits(homeTeam: Team, awayTeam: Team): KitInfo {
  const home = homeKit(homeTeam)
  const away = resolveAwayKit(homeTeam, awayTeam)
  const clash = !!home && !!away && sameColor(home.colorPrimary, away.colorPrimary)
  return { home, away, clash }
}
