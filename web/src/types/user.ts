import type { TeamMemberRole } from '@/types/team'

export type UserRole = 'Member' | 'Admin'

/** Mirrors the backend `UserDto` (never carries password data). */
export interface User {
  id: string
  userName: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
  /** The role picked at sign-up; pre-selects the role when joining or founding a team. */
  primaryRole: TeamMemberRole
  createdAt: string
  active: boolean
  hasPhoto: boolean
}
