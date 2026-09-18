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

/** A team's first kit by default, or its second if that's the only one it has. The challenger
 *  may override this to its second kit - see `ChallengeWizard.vue`'s `challengerKitPreview` -
 *  this is only the unoverridden default. */
export function resolveDefaultKit(team: Team): ResolvedKit | null {
  return firstKit(team) ?? secondKit(team)
}

/**
 * The opponent's first kit, unless its primary colour matches `challengerKit`'s - then its
 * second kit if that avoids the clash; otherwise falls back to its first kit anyway (accepting
 * the clash) if it has one, else null (no kit configured at all). Takes the challenger's
 * *resolved* kit (not the team) so it reacts correctly to a manually chosen kit, not just its
 * default. A client-side preview of the backend's own resolution
 * (`SendChallengeHandler.ResolveOpponentKitSlot`) - the server is the source of truth, this is
 * only so the wizard can show the result before sending. Never returns null just because of a
 * clash - a clash is informational, never blocking.
 */
export function resolveOpponentKit(challengerKit: ResolvedKit | null, opponent: Team): ResolvedKit | null {
  const first = firstKit(opponent)

  if (!challengerKit) return first

  if (first && !sameColor(first.colorPrimary, challengerKit.colorPrimary)) return first

  const second = secondKit(opponent)
  if (second && !sameColor(second.colorPrimary, challengerKit.colorPrimary)) return second

  return first
}

export function resolveKits(homeTeam: Team, awayTeam: Team): KitInfo {
  const home = resolveDefaultKit(homeTeam)
  const away = resolveOpponentKit(home, awayTeam)
  const clash = !!home && !!away && sameColor(home.colorPrimary, away.colorPrimary)
  return { home, away, clash }
}
