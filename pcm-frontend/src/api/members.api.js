/**
 * Members API - Quản lý thành viên
 */
import api from './axios'

export const membersApi = {
  getAll(params = {}) {
    return api.get('/members', { params })
  },

  getById(id) {
    return api.get(`/members/${id}`)
  },

  create(data) {
    return api.post('/members', data)
  },

  update(id, data) {
    return api.put(`/members/${id}`, data)
  },

  delete(id) {
    return api.delete(`/members/${id}`)
  },

  getTopRanking(limit = 5) {
    return api.get('/members/top-ranking', { params: { limit } })
  },

  getStats() {
    return api.get('/members/stats')
  }
}

export default membersApi
