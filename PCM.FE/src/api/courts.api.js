/**
 * Courts API - Quản lý sân
 */
import api from './axios'

export const courtsApi = {
  getAll(params = {}) {
    return api.get('/courts', { params })
  },

  getById(id) {
    return api.get(`/courts/${id}`)
  },

  create(data) {
    return api.post('/courts', data)
  },

  update(id, data) {
    return api.put(`/courts/${id}`, data)
  },

  delete(id) {
    return api.delete(`/courts/${id}`)
  },

  getActive() {
    return api.get('/courts/active')
  }
}

export default courtsApi
