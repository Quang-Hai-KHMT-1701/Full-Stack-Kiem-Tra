/**
 * News API - Tin tức
 */
import api from './axios'

export const newsApi = {
  getAll(params = {}) {
    return api.get('/news', { params })
  },

  getById(id) {
    return api.get(`/news/${id}`)
  },

  create(data) {
    return api.post('/news', data)
  },

  update(id, data) {
    return api.put(`/news/${id}`, data)
  },

  delete(id) {
    return api.delete(`/news/${id}`)
  },

  getPinned() {
    return api.get('/news', { params: { isPinned: true } })
  },

  togglePin(id, isPinned) {
    return api.patch(`/news/${id}/pin`, { isPinned })
  }
}

export default newsApi
