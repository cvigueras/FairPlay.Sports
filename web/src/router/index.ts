import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

/** Team-listing screens a team's detail page can be reached from. */
const TEAM_DETAIL_ORIGINS = new Set(['teams', 'my-teams', 'standings'])

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('@/views/ProfileView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/my-teams',
      name: 'my-teams',
      component: () => import('@/views/MyTeamsView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/teams',
      name: 'teams',
      component: () => import('@/views/TeamsView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/teams/:id',
      name: 'team-detail',
      component: () => import('@/views/TeamDetailView.vue'),
      meta: { requiresAuth: true },
      props: true,
    },
    {
      path: '/standings',
      name: 'standings',
      component: () => import('@/views/StandingsView.vue'),
      meta: { requiresAuth: true },
    },
    { path: '/dashboard', redirect: '/profile' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.meta.guestOnly && auth.isAuthenticated) {
    return { name: 'home' }
  }
})

// Remembers the last team-listing screen visited, so a team detail page's
// breadcrumb can point back to wherever the user actually came from
// (Teams, My teams or Standings) instead of a hardcoded parent.
router.afterEach((to) => {
  if (typeof to.name === 'string' && TEAM_DETAIL_ORIGINS.has(to.name)) {
    useUiStore().lastListRoute = { name: to.name, path: to.fullPath }
  }
})

export default router
