/**
 * Bookings API - Đặt sân
 */
import api from './axios'

export const bookingsApi = {
  getAll(params = {}) {
    return api.get('/bookings', { params })
  },

  getById(id) {
    return api.get(`/bookings/${id}`)
  },

  create(data) {
    return api.post('/bookings', data)
  },

  update(id, data) {
    return api.put(`/bookings/${id}`, data)
  },

  cancel(id) {
    return api.post(`/bookings/${id}/cancel`)
  },

  delete(id) {
    return api.delete(`/bookings/${id}`)
  },

  getAvailableSlots(params) {
    return api.get('/bookings/available-slots', { params })
  },

  getByDate(date) {
    return api.get('/bookings/by-date', { params: { date } })
  },

  getMyBookings(params = {}) {
    return api.get('/bookings/my-bookings', { params })
  },

  confirm(id) {
    return api.post(`/bookings/${id}/confirm`)
  },

  reject(id, reason) {
    return api.post(`/bookings/${id}/reject`, { reason })
  }
}

export default bookingsApi
