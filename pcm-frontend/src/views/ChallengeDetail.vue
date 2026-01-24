<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Back button & Header -->
      <div class="flex items-center gap-4">
        <button @click="$router.back()" class="p-2 hover:bg-gray-100 rounded-lg">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <div>
          <h1 class="page-title">Chi tiết thử thách</h1>
        </div>
      </div>
      
      <div v-if="loading" class="flex justify-center py-12">
        <LoadingSpinner size="lg" text="Đang tải..." />
      </div>
      
      <div v-else-if="!challenge" class="text-center py-12">
        <p class="text-gray-500">Không tìm thấy thử thách</p>
      </div>
      
      <template v-else>
        <div class="grid lg:grid-cols-3 gap-6">
          <!-- Main Info -->
          <div class="lg:col-span-2 space-y-6">
            <div class="card">
              <div class="p-6">
                <div class="flex items-start justify-between mb-4">
                  <h2 class="text-2xl font-bold text-gray-900">{{ challenge.title }}</h2>
                  <span :class="getStatusBadgeClass(challenge.status)" class="text-base px-3 py-1">
                    {{ getStatusLabel(challenge.status) }}
                  </span>
                </div>
                
                <p class="text-gray-600 mb-6">{{ challenge.description || 'Không có mô tả' }}</p>
                
                <div class="grid sm:grid-cols-2 gap-4">
                  <div class="p-4 bg-gray-50 rounded-xl">
                    <p class="text-sm text-gray-500">💰 Phí tham gia</p>
                    <p class="text-xl font-bold text-gray-900">{{ formatCurrency(challenge.entryFee || 0) }}</p>
                  </div>
                  <div class="p-4 bg-primary-50 rounded-xl">
                    <p class="text-sm text-gray-500">🎁 Giải thưởng</p>
                    <p class="text-xl font-bold text-primary-600">{{ formatCurrency(challenge.prizePool || 0) }}</p>
                  </div>
                  <div class="p-4 bg-gray-50 rounded-xl">
                    <p class="text-sm text-gray-500">📅 Ngày bắt đầu</p>
                    <p class="text-lg font-semibold text-gray-900">{{ formatDate(challenge.startDate) }}</p>
                  </div>
                  <div class="p-4 bg-gray-50 rounded-xl">
                    <p class="text-sm text-gray-500">📅 Ngày kết thúc</p>
                    <p class="text-lg font-semibold text-gray-900">{{ formatDate(challenge.endDate) }}</p>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Participants -->
            <div class="card">
              <div class="card-header flex items-center justify-between">
                <h3 class="font-semibold">
                  Danh sách người tham gia ({{ participants.length }}/{{ challenge.maxParticipants || '∞' }})
                </h3>
                <button 
                  v-if="authStore.isAdmin && challenge.status === 'Open' && participants.length >= 2"
                  @click="autoDivideTeams"
                  class="btn-secondary btn-sm"
                >
                  🎲 Chia đội tự động
                </button>
              </div>
              <div class="card-body">
                <div v-if="participants.length === 0" class="text-center py-8 text-gray-500">
                  Chưa có ai tham gia
                </div>
                <div v-else class="grid sm:grid-cols-2 gap-3">
                  <div 
                    v-for="participant in participants" 
                    :key="participant.id"
                    class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
                  >
                    <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center">
                      <span class="text-primary-700 font-medium">
                        {{ getInitials(participant.member?.fullName) }}
                      </span>
                    </div>
                    <div>
                      <p class="font-medium">{{ participant.member?.fullName }}</p>
                      <p v-if="participant.team" class="text-sm text-gray-500">
                        Đội {{ participant.team }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Sidebar -->
          <div class="space-y-6">
            <!-- Actions -->
            <div class="card p-6">
              <h3 class="font-semibold mb-4">Hành động</h3>
              <div class="space-y-3">
                <button 
                  v-if="challenge.status === 'Open' && !isJoined"
                  @click="joinChallenge"
                  class="btn-primary w-full"
                  :disabled="joining"
                >
                  {{ joining ? 'Đang xử lý...' : 'Tham gia thử thách' }}
                </button>
                <button 
                  v-if="challenge.status === 'Open' && isJoined"
                  @click="leaveChallenge"
                  class="btn-danger w-full"
                  :disabled="leaving"
                >
                  {{ leaving ? 'Đang xử lý...' : 'Rời khỏi thử thách' }}
                </button>
                
                <template v-if="authStore.isAdmin">
                  <button 
                    v-if="challenge.status === 'Open'"
                    @click="startChallenge"
                    class="btn-success w-full"
                  >
                    ▶️ Bắt đầu
                  </button>
                  <button 
                    v-if="challenge.status === 'Ongoing'"
                    @click="finishChallenge"
                    class="btn-primary w-full"
                  >
                    ✅ Kết thúc
                  </button>
                  <button 
                    v-if="['Open', 'Ongoing'].includes(challenge.status)"
                    @click="cancelChallenge"
                    class="btn-danger w-full"
                  >
                    ❌ Hủy thử thách
                  </button>
                </template>
              </div>
            </div>
            
            <!-- Creator info -->
            <div class="card p-6">
              <h3 class="font-semibold mb-4">Thông tin</h3>
              <div class="space-y-3 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-500">Loại</span>
                  <span class="font-medium">{{ challenge.type || 'Standard' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Ngày tạo</span>
                  <span class="font-medium">{{ formatDate(challenge.createdAt) }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>
  </MainLayout>
</template>

<script setup>
import { challengesApi } from '@/api/challenges.api'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { formatCurrency, formatDate, getStatusLabel } from '@/utils/format'
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useToast } from 'vue-toastification'

const route = useRoute()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const challenge = ref(null)
const participants = ref([])
const joining = ref(false)
const leaving = ref(false)

const isJoined = computed(() => {
  return participants.value.some(p => p.memberId === authStore.user?.id)
})

const getStatusBadgeClass = (status) => {
  const classes = {
    'Open': 'badge-success',
    'Ongoing': 'badge-warning',
    'Finished': 'badge-info',
    'Cancelled': 'badge-danger'
  }
  return classes[status] || 'badge-info'
}

const getInitials = (name) => {
  if (!name) return '?'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

const fetchChallenge = async () => {
  loading.value = true
  try {
    const response = await challengesApi.getById(route.params.id)
    challenge.value = response.data
    
    const participantsResponse = await challengesApi.getParticipants(route.params.id)
    participants.value = participantsResponse.data?.data || participantsResponse.data || []
  } catch (error) {
    toast.error('Không thể tải thông tin thử thách')
  } finally {
    loading.value = false
  }
}

const joinChallenge = async () => {
  joining.value = true
  try {
    await challengesApi.join(route.params.id)
    toast.success('Đã tham gia thử thách!')
    fetchChallenge()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tham gia')
  } finally {
    joining.value = false
  }
}

const leaveChallenge = async () => {
  leaving.value = true
  try {
    await challengesApi.leave(route.params.id)
    toast.success('Đã rời khỏi thử thách')
    fetchChallenge()
  } catch (error) {
    toast.error('Không thể rời khỏi')
  } finally {
    leaving.value = false
  }
}

const autoDivideTeams = async () => {
  try {
    await challengesApi.autoDivideTeams(route.params.id)
    toast.success('Đã chia đội thành công!')
    fetchChallenge()
  } catch (error) {
    toast.error('Không thể chia đội')
  }
}

const startChallenge = async () => {
  try {
    await challengesApi.start(route.params.id)
    toast.success('Đã bắt đầu thử thách!')
    fetchChallenge()
  } catch (error) {
    toast.error('Không thể bắt đầu')
  }
}

const finishChallenge = async () => {
  try {
    await challengesApi.finish(route.params.id)
    toast.success('Đã kết thúc thử thách!')
    fetchChallenge()
  } catch (error) {
    toast.error('Không thể kết thúc')
  }
}

const cancelChallenge = async () => {
  if (!confirm('Bạn có chắc chắn muốn hủy thử thách này?')) return
  
  try {
    await challengesApi.cancel(route.params.id)
    toast.success('Đã hủy thử thách')
    fetchChallenge()
  } catch (error) {
    toast.error('Không thể hủy')
  }
}

onMounted(() => {
  fetchChallenge()
})
</script>
