/**
 * Axios instance với JWT interceptor
 * - Tự động gắn Authorization header
 * - Xử lý 401 Unauthorized -> redirect login
 */
import router from '@/router'
import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5211/api',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor - Gắn JWT token vào header
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - Xử lý lỗi 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      
      if (router.currentRoute.value.path !== '/login') {
        router.push({
          path: '/login',
          query: { redirect: router.currentRoute.value.fullPath }
        })
      }
    }
    
    return Promise.reject(error)
  }
)

export default api
