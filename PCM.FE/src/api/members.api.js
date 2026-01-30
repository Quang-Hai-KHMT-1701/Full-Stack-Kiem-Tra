/**
 * Members API - Quản lý thành viên
 */
import api from './axios'

export const membersApi = {
  // Lấy danh sách thành viên với filter, search, pagination
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

  // Cập nhật trạng thái (kích hoạt/vô hiệu hóa)
  updateStatus(id, isActive) {
    return api.patch(`/members/${id}/status`, { isActive })
  },

  // Cập nhật rank
  updateRank(id, rankLevel) {
    return api.patch(`/members/${id}/rank`, { rankLevel })
  },

  getTopRanking(limit = 5) {
    return api.get('/members/top-ranking', { params: { limit } })
  },

  getStats() {
    return api.get('/members/stats')
  }
}

export default membersApi
