import type { User } from '@/types/user'

/** Backend `TeamMember.MaxDisplayNameLength`. */
const MAX_DISPLAY_NAME_LENGTH = 100

/**
 * "First Last" for a user, falling back to the username for accounts created
 * before names were collected at sign-up (they have empty first/last names).
 * Capped to what a team member's display name accepts.
 */
export function fullName(user: Pick<User, 'firstName' | 'lastName' | 'userName'> | null | undefined): string {
  if (!user) return ''
  const name = [user.firstName, user.lastName]
    .map((part) => part?.trim())
    .filter(Boolean)
    .join(' ')
  return (name || user.userName).slice(0, MAX_DISPLAY_NAME_LENGTH)
}
