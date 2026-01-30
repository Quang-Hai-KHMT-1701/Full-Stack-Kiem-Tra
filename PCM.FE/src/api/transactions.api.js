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
  getAll(params = {}) {
    return api.get('/transaction-categories', { params })
  },

  getById(id) {
    return api.get(`/transaction-categories/${id}`)
  },

  create(data) {
    return api.post('/transaction-categories', data)
  },

  update(id, data) {
    return api.put(`/transaction-categories/${id}`, data)
  },

  delete(id) {
    return api.delete(`/transaction-categories/${id}`)
  }
}

// Also add to transactionsApi for backward compatibility
transactionsApi.getCategories = categoriesApi.getAll

export default transactionsApi
