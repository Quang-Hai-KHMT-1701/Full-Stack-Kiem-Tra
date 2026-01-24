<template>
  <MainLayout>
    <div class="space-y-6">
      <div class="flex items-center gap-4">
        <button @click="$router.back()" class="p-2 hover:bg-gray-100 rounded-lg">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <h1 class="page-title">Chi tiết thành viên</h1>
      </div>
      
      <div v-if="loading" class="flex justify-center py-12">
        <LoadingSpinner size="lg" text="Đang tải..." />
      </div>
      
      <div v-else-if="!member" class="text-center py-12">
        <p class="text-gray-500">Không tìm thấy thành viên</p>
      </div>
      
      <div v-else class="grid lg:grid-cols-3 gap-6">
        <!-- Profile card -->
        <div class="card p-6 text-center">
          <div class="w-24 h-24 bg-primary-100 rounded-full flex items-center justify-center mx-auto">
            <span class="text-3xl font-bold text-primary-700">{{ getInitials(member.fullName) }}</span>
          </div>
          <h2 class="text-xl font-bold text-gray-900 mt-4">{{ member.fullName }}</h2>
          <p class="text-gray-500">{{ member.email }}</p>
          <div class="flex justify-center gap-2 mt-3">
            <span 
              v-for="role in member.roles || ['Member']" 
              :key="role"
              :class="getRoleBadgeClass(role)"
            >
              {{ getRoleLabel(role) }}
            </span>
          </div>
          
          <div class="mt-6 pt-6 border-t border-gray-100">
            <div class="text-center">
              <p class="text-3xl font-bold text-primary-600">{{ member.rankingPoints || 0 }}</p>
              <p class="text-sm text-gray-500">Điểm xếp hạng</p>
            </div>
          </div>
        </div>
        
        <!-- Info & Stats -->
        <div class="lg:col-span-2 space-y-6">
          <div class="card">
            <div class="card-header">
              <h3 class="font-semibold">Thông tin cá nhân</h3>
            </div>
            <div class="card-body grid sm:grid-cols-2 gap-4">
              <div>
                <label class="text-sm text-gray-500">Họ và tên</label>
                <p class="font-medium">{{ member.fullName }}</p>
              </div>
              <div>
                <label class="text-sm text-gray-500">Email</label>
                <p class="font-medium">{{ member.email }}</p>
              </div>
              <div>
                <label class="text-sm text-gray-500">Số điện thoại</label>
                <p class="font-medium">{{ member.phoneNumber || 'Chưa cập nhật' }}</p>
              </div>
              <div>
                <label class="text-sm text-gray-500">Ngày tham gia</label>
                <p class="font-medium">{{ formatDate(member.createdAt) }}</p>
              </div>
            </div>
          </div>
          
          <div class="card">
            <div class="card-header">
              <h3 class="font-semibold">Thống kê thi đấu</h3>
            </div>
            <div class="card-body">
              <div class="grid grid-cols-3 gap-4 text-center">
                <div class="p-4 bg-green-50 rounded-xl">
                  <p class="text-2xl font-bold text-green-600">{{ member.wins || 0 }}</p>
                  <p class="text-sm text-gray-600">Thắng</p>
                </div>
                <div class="p-4 bg-red-50 rounded-xl">
                  <p class="text-2xl font-bold text-red-600">{{ member.losses || 0 }}</p>
                  <p class="text-sm text-gray-600">Thua</p>
                </div>
                <div class="p-4 bg-blue-50 rounded-xl">
                  <p class="text-2xl font-bold text-blue-600">{{ member.totalMatches || 0 }}</p>
                  <p class="text-sm text-gray-600">Tổng trận</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </MainLayout>
</template>

<script setup>
import { membersApi } from '@/api/members.api'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { formatDate } from '@/utils/format'
import { getRoleBadgeClass, getRoleLabel } from '@/utils/roles'
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useToast } from 'vue-toastification'

const route = useRoute()
const toast = useToast()

const loading = ref(true)
const member = ref(null)

const getInitials = (name) => {
  if (!name) return '?'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

onMounted(async () => {
  try {
    const response = await membersApi.getById(route.params.id)
    member.value = response.data
  } catch (error) {
    toast.error('Không thể tải thông tin thành viên')
  } finally {
    loading.value = false
  }
})
</script>
