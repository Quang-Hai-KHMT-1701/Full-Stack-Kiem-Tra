/**
 * Vue Router với navigation guards
 */
import { useAuthStore } from '@/stores/auth.store'
import { createRouter, createWebHistory } from 'vue-router'

// Lazy load components
const Login = () => import('@/views/Login.vue')
const Register = () => import('@/views/Register.vue')
const Dashboard = () => import('@/views/Dashboard.vue')
const Members = () => import('@/views/Members.vue')
const MemberDetail = () => import('@/views/MemberDetail.vue')
const News = () => import('@/views/News.vue')
const NewsDetail = () => import('@/views/NewsDetail.vue')
const Courts = () => import('@/views/Courts.vue')
const Bookings = () => import('@/views/Bookings.vue')
const Challenges = () => import('@/views/Challenges.vue')
const ChallengeDetail = () => import('@/views/ChallengeDetail.vue')
const Matches = () => import('@/views/Matches.vue')
const Transactions = () => import('@/views/Transactions.vue')
const TransactionCategories = () => import('@/views/TransactionCategories.vue')
const Profile = () => import('@/views/Profile.vue')
const NotFound = () => import('@/views/NotFound.vue')

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { guest: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
    meta: { guest: true }
  },
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true }
  },
  {
    path: '/members',
    name: 'Members',
    component: Members,
    meta: { requiresAuth: true }
  },
  {
    path: '/members/:id',
    name: 'MemberDetail',
    component: MemberDetail,
    meta: { requiresAuth: true }
  },
  {
    path: '/news',
    name: 'News',
    component: News,
    meta: { requiresAuth: true }
  },
  {
    path: '/news/:id',
    name: 'NewsDetail',
    component: NewsDetail,
    meta: { requiresAuth: true }
  },
  {
    path: '/courts',
    name: 'Courts',
    component: Courts,
    meta: { requiresAuth: true, roles: ['Admin'] }
  },
  {
    path: '/bookings',
    name: 'Bookings',
    component: Bookings,
    meta: { requiresAuth: true }
  },
  {
    path: '/challenges',
    name: 'Challenges',
    component: Challenges,
    meta: { requiresAuth: true }
  },
  {
    path: '/challenges/:id',
    name: 'ChallengeDetail',
    component: ChallengeDetail,
    meta: { requiresAuth: true }
  },
  {
    path: '/matches',
    name: 'Matches',
    component: Matches,
    meta: { requiresAuth: true, roles: ['Admin', 'Referee'] }
  },
  {
    path: '/transactions',
    name: 'Transactions',
    component: Transactions,
    meta: { requiresAuth: true, roles: ['Admin', 'Treasurer'] }
  },
  {
    path: '/transaction-categories',
    name: 'TransactionCategories',
    component: TransactionCategories,
    meta: { requiresAuth: true, roles: ['Admin'] }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: Profile,
    meta: { requiresAuth: true }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFound
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Initialize auth if needed
  if (!authStore.token && localStorage.getItem('token')) {
    authStore.initializeAuth()
  }

  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }

  // Check if route is for guests only (login, register)
  if (to.meta.guest && authStore.isAuthenticated) {
    return next('/dashboard')
  }

  // Check role requirements
  if (to.meta.roles && to.meta.roles.length > 0) {
    const hasRequiredRole = to.meta.roles.some(role => authStore.hasRole(role))
    if (!hasRequiredRole) {
      return next('/dashboard')
    }
  }

  next()
})

export default router
