import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { http } from '@/lib/http'
import type { User } from '@/types/user'

interface AuthResponse {
  accessToken: string
  expiresAtUtc: string
  user: User
}

export interface RegisterPayload {
  userName: string
  email: string
  password: string
}

/**
 * Talks to the FairPlay backend auth endpoints. The access token lives only in
 * memory; the refresh token is an HttpOnly cookie the browser stores and the
 * API rotates. On a full page reload `tryRefresh()` swaps that cookie for a
 * fresh access token so the session survives.
 */
export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const currentUser = ref<User | null>(null)
  const isAuthenticated = computed(() => currentUser.value !== null)

  function apply(response: AuthResponse): void {
    accessToken.value = response.accessToken
    currentUser.value = response.user
  }

  function clear(): void {
    accessToken.value = null
    currentUser.value = null
  }

  async function login(email: string, password: string): Promise<void> {
    apply(await http.post<AuthResponse>('/api/auth/login', { email, password }))
  }

  /** Creates the account. Does not sign in - the caller sends the user to login. */
  async function register(payload: RegisterPayload): Promise<User> {
    return http.post<User>('/api/users', payload)
  }

  /** Best-effort session restore from the refresh cookie. Never throws. */
  async function tryRefresh(): Promise<void> {
    try {
      apply(await http.post<AuthResponse>('/api/auth/refresh'))
    } catch {
      clear()
    }
  }

  async function logout(): Promise<void> {
    try {
      await http.post<void>('/api/auth/logout', undefined, { token: accessToken.value })
    } catch {
      // The cookie may already be gone; clear the client either way.
    } finally {
      clear()
    }
  }

  return { accessToken, currentUser, isAuthenticated, login, register, tryRefresh, logout }
})
