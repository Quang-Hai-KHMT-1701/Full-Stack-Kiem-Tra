/**
 * Auth API - Đăng nhập, đăng ký
 */
import api from './axios'

export const authApi = {
  login(credentials) {
    return api.post('/auth/login', credentials)
  },

  register(userData) {
    return api.post('/auth/register', userData)
  },

  getCurrentUser() {
    return api.get('/auth/me')
  },

  changePassword(data) {
    return api.post('/auth/change-password', data)
  }
}

export default authApi
