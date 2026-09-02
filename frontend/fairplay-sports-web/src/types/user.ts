export type UserRole = 'Member' | 'Admin'

/** Mirrors the backend `UserDto` (never carries password data). */
export interface User {
  id: string
  userName: string
  email: string
  team: string
  role: UserRole
  createdAt: string
  active: boolean
}
