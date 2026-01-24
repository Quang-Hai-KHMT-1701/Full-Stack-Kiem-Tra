/**
 * Challenges API - Quản lý thử thách/giải đấu
 */
import api from './axios'

export const challengesApi = {
  getAll(params = {}) {
    return api.get('/challenges', { params })
  },

  getById(id) {
    return api.get(`/challenges/${id}`)
  },

  create(data) {
    return api.post('/challenges', data)
  },

  update(id, data) {
    return api.put(`/challenges/${id}`, data)
  },

  delete(id) {
    return api.delete(`/challenges/${id}`)
  },

  join(id, data = {}) {
    return api.post(`/challenges/${id}/join`, data)
  },

  leave(id) {
    return api.post(`/challenges/${id}/leave`)
  },

  autoDivideTeams(id) {
    return api.post(`/challenges/${id}/auto-divide-teams`)
  },

  getParticipants(id) {
    return api.get(`/challenges/${id}/participants`)
  },

  start(id) {
    return api.post(`/challenges/${id}/start`)
  },

  finish(id) {
    return api.post(`/challenges/${id}/finish`)
  },

  cancel(id) {
    return api.post(`/challenges/${id}/cancel`)
  }
}

export default challengesApi
