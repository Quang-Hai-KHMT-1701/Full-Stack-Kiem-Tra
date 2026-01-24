/**
 * Auth Store - Quản lý authentication state
 */
import { authApi } from '@/api/auth.api'
import router from '@/router'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref(null)
  const user = ref(null)
  const loading = ref(false)

  // Getters
  const isAuthenticated = computed(() => !!token.value)
  
  const roles = computed(() => {
    if (!user.value?.roles) return []
    return Array.isArray(user.value.roles) ? user.value.roles : [user.value.roles]
  })

  const isAdmin = computed(() => roles.value.includes('Admin'))
  const isReferee = computed(() => roles.value.includes('Referee') || isAdmin.value)
  const isTreasurer = computed(() => roles.value.includes('Treasurer') || isAdmin.value)
  const isMember = computed(() => roles.value.includes('Member') || isAdmin.value)

  // Helper function to check role
  const hasRole = (role) => {
    if (role === 'Admin') return isAdmin.value
    if (role === 'Referee') return isReferee.value
    if (role === 'Treasurer') return isTreasurer.value
    return roles.value.includes(role) || isAdmin.value
  }

  // Actions
  const initializeAuth = () => {
    const savedToken = localStorage.getItem('token')
    const savedUser = localStorage.getItem('user')
    
    if (savedToken) {
      token.value = savedToken
    }
    
    if (savedUser) {
      try {
        user.value = JSON.parse(savedUser)
      } catch (e) {
        console.error('Error parsing saved user:', e)
        localStorage.removeItem('user')
      }
    }
  }

  const login = async (credentials) => {
    loading.value = true
    try {
      const response = await authApi.login(credentials)
      const data = response.data
      
      // Lưu token và user info
      token.value = data.token
      user.value = data.user || {
        id: data.userId,
        email: data.email,
        fullName: data.fullName,
        roles: data.roles
      }
      
      // Persist to localStorage
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(user.value))
      
      return { success: true }
    } catch (error) {
      const message = error.response?.data?.message || 'Đăng nhập thất bại'
      return { success: false, message }
    } finally {
      loading.value = false
    }
  }

  const logout = () => {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    router.push('/login')
  }

  const updateUser = (userData) => {
    user.value = { ...user.value, ...userData }
    localStorage.setItem('user', JSON.stringify(user.value))
  }

  return {
    // State
    token,
    user,
    loading,
    // Getters
    isAuthenticated,
    roles,
    isAdmin,
    isReferee,
    isTreasurer,
    isMember,
    // Actions
    hasRole,
    initializeAuth,
    login,
    logout,
    updateUser
  }
})
