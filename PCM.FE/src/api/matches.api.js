/**
 * Matches API - Quản lý trận đấu
 */
import api from './axios'

export const matchesApi = {
  getAll(params = {}) {
    return api.get('/matches', { params })
  },

  getById(id) {
    return api.get(`/matches/${id}`)
  },

  create(data) {
    return api.post('/matches', data)
  },

  update(id, data) {
    return api.put(`/matches/${id}`, data)
  },

  delete(id) {
    return api.delete(`/matches/${id}`)
  },

  getByMember(memberId, params = {}) {
    return api.get(`/matches/member/${memberId}`, { params })
  },

  getByChallenge(challengeId) {
    return api.get(`/matches/challenge/${challengeId}`)
  },

  getRecent(limit = 10) {
    return api.get('/matches/recent', { params: { limit } })
  }
}

export default matchesApi
