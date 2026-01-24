/**
 * Common Store - Cache dữ liệu dùng chung
 */
import { courtsApi } from '@/api/courts.api'
import { membersApi } from '@/api/members.api'
import { categoriesApi } from '@/api/transactions.api'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useCommonStore = defineStore('common', () => {
  // State
  const courts = ref([])
  const members = ref([])
  const categories = ref([])
  const courtsLoading = ref(false)
  const membersLoading = ref(false)
  const categoriesLoading = ref(false)

  // Actions
  const fetchCourts = async (force = false) => {
    if (courts.value.length > 0 && !force) return courts.value
    
    courtsLoading.value = true
    try {
      const response = await courtsApi.getAll()
      courts.value = response.data.data || response.data || []
      return courts.value
    } catch (error) {
      console.error('Error fetching courts:', error)
      return []
    } finally {
      courtsLoading.value = false
    }
  }

  const fetchMembers = async (force = false) => {
    if (members.value.length > 0 && !force) return members.value
    
    membersLoading.value = true
    try {
      const response = await membersApi.getAll()
      members.value = response.data.data || response.data || []
      return members.value
    } catch (error) {
      console.error('Error fetching members:', error)
      return []
    } finally {
      membersLoading.value = false
    }
  }

  const fetchCategories = async (force = false) => {
    if (categories.value.length > 0 && !force) return categories.value
    
    categoriesLoading.value = true
    try {
      const response = await categoriesApi.getAll()
      categories.value = response.data.data || response.data || []
      return categories.value
    } catch (error) {
      console.error('Error fetching categories:', error)
      return []
    } finally {
      categoriesLoading.value = false
    }
  }

  const clearCache = () => {
    courts.value = []
    members.value = []
    categories.value = []
  }

  return {
    // State
    courts,
    members,
    categories,
    courtsLoading,
    membersLoading,
    categoriesLoading,
    // Actions
    fetchCourts,
    fetchMembers,
    fetchCategories,
    clearCache
  }
})
