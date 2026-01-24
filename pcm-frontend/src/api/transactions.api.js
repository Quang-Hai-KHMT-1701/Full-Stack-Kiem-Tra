/**
 * Transactions API - Quản lý thu chi
 */
import api from './axios'

export const transactionsApi = {
  getAll(params = {}) {
    return api.get('/transactions', { params })
  },

  getById(id) {
    return api.get(`/transactions/${id}`)
  },

  create(data) {
    return api.post('/transactions', data)
  },

  update(id, data) {
    return api.put(`/transactions/${id}`, data)
  },

  delete(id) {
    return api.delete(`/transactions/${id}`)
  },

  getSummary(params = {}) {
    return api.get('/transactions/summary', { params })
  },

  getByCategory(params = {}) {
    return api.get('/transactions/by-category', { params })
  },

  export(params = {}) {
    return api.get('/transactions/export', {
      params,
      responseType: 'blob'
    })
  }
}

export const categoriesApi = {
  getAll() {
    return api.get('/categories')
  },

  getById(id) {
    return api.get(`/categories/${id}`)
  },

  create(data) {
    return api.post('/categories', data)
  },

  update(id, data) {
    return api.put(`/categories/${id}`, data)
  },

  delete(id) {
    return api.delete(`/categories/${id}`)
  }
}

export default transactionsApi
