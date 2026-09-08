export type UserRole = 'Member' | 'Admin'

/** Mirrors the backend `UserDto` (never carries password data). */
export interface User {
  id: string
  userName: string
  email: string
  teamId: string | null
  role: UserRole
  createdAt: string
  active: boolean
}
