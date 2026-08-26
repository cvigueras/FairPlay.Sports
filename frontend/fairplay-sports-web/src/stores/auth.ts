import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export interface DummyUser {
  name: string
  email: string
}

/**
 * Dummy auth store: no real backend call is made. Login/register just
 * simulate network latency and store a fake user in memory (Pinia state
 * is not persisted, so a page refresh logs the user out again - by design).
 */
export const useAuthStore = defineStore('auth', () => {
  const currentUser = ref<DummyUser | null>(null)
  const isAuthenticated = computed(() => currentUser.value !== null)

  function login(email: string): Promise<void> {
    return new Promise((resolve) => {
      setTimeout(() => {
        currentUser.value = { name: email.split('@')[0] ?? email, email }
        resolve()
      }, 400)
    })
  }

  function register(name: string, email: string): Promise<void> {
    return new Promise((resolve) => {
      setTimeout(() => {
        currentUser.value = { name, email }
        resolve()
      }, 400)
    })
  }

  function logout(): void {
    currentUser.value = null
  }

  return { currentUser, isAuthenticated, login, register, logout }
})
