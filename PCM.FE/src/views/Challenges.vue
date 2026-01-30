<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Bảng thử thách</h1>
          <p class="page-subtitle">Tham gia các thử thách và giải đấu</p>
        </div>
        <button @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Tạo thử thách
        </button>
      </div>
      
      <!-- Filters -->
      <div class="flex flex-wrap gap-2">
        <button 
          v-for="status in statusFilters" 
          :key="status.value"
          @click="selectedStatus = status.value"
          :class="[
            'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
            selectedStatus === status.value 
              ? 'bg-primary-600 text-white' 
              : 'bg-white text-gray-700 hover:bg-gray-100 border border-gray-200'
          ]"
        >
          {{ status.label }}
        </button>
      </div>
      
      <!-- Challenges grid -->
      <div v-if="loading" class="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="i in 6" :key="i" class="card p-6">
          <div class="skeleton h-6 w-3/4 mb-4"></div>
          <div class="skeleton h-4 w-full mb-2"></div>
          <div class="skeleton h-4 w-2/3 mb-4"></div>
          <div class="flex gap-2">
            <div class="skeleton h-8 w-20"></div>
            <div class="skeleton h-8 w-20"></div>
          </div>
        </div>
      </div>
      
      <div v-else-if="filteredChallenges.length === 0" class="card p-12 text-center">
        <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
        </svg>
        <h3 class="text-lg font-medium text-gray-900">Chưa có thử thách nào</h3>
        <p class="text-gray-500 mt-1">Hãy tạo thử thách đầu tiên!</p>
      </div>
      
      <div v-else class="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div 
          v-for="challenge in filteredChallenges" 
          :key="challenge.id"
          class="card hover:shadow-lg transition-all cursor-pointer"
          @click="$router.push(`/challenges/${challenge.id}`)"
        >
          <div class="p-6">
            <div class="flex items-start justify-between mb-3">
              <h3 class="font-bold text-lg text-gray-900">{{ challenge.title }}</h3>
              <span :class="getStatusBadgeClass(challenge.status)">
                {{ getStatusLabel(challenge.status) }}
              </span>
            </div>
            
            <p class="text-gray-600 text-sm line-clamp-2 mb-4">
              {{ challenge.description || 'Không có mô tả' }}
            </p>
            
            <div class="space-y-2 text-sm">
              <div class="flex items-center justify-between">
                <span class="text-gray-500">💰 Phí tham gia:</span>
                <span class="font-semibold text-gray-900">{{ formatCurrency(challenge.entryFee || 0) }}</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-gray-500">🎁 Giải thưởng:</span>
                <span class="font-semibold text-primary-600">{{ formatCurrency(challenge.prizePool || 0) }}</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-gray-500">👥 Người tham gia:</span>
                <span class="font-medium">
                  {{ challenge.participantCount || 0 }}/{{ challenge.maxParticipants || '∞' }}
                </span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-gray-500">📅 Thời gian:</span>
                <span class="text-gray-700">{{ formatDate(challenge.startDate) }}</span>
              </div>
            </div>
          </div>
          
          <div class="px-6 py-4 bg-gray-50 border-t border-gray-100">
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-500">{{ challenge.type || 'Standard' }}</span>
              <button 
                v-if="challenge.status === 'Open'"
                @click.stop="joinChallenge(challenge)"
                class="btn-primary btn-sm"
              >
                Tham gia
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Create Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showCreateModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
            <div class="px-6 py-4 border-b border-gray-100 sticky top-0 bg-white">
              <h3 class="text-lg font-semibold">Tạo thử thách mới</h3>
            </div>
            <form @submit.prevent="submitChallenge" class="p-6 space-y-4">
              <div>
                <label class="form-label">Tiêu đề *</label>
                <input v-model="form.title" type="text" class="form-input" required placeholder="VD: Giải đấu cuối tuần" />
              </div>
              <div>
                <label class="form-label">Mô tả</label>
                <textarea v-model="form.description" class="form-input" rows="3" placeholder="Mô tả về thử thách..."></textarea>
              </div>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="form-label">Phí tham gia (VNĐ)</label>
                  <input v-model.number="form.entryFee" type="number" class="form-input" min="0" />
                </div>
                <div>
                  <label class="form-label">Giải thưởng (VNĐ)</label>
                  <input v-model.number="form.prizePool" type="number" class="form-input" min="0" />
                </div>
              </div>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="form-label">Ngày bắt đầu</label>
                  <input v-model="form.startDate" type="date" class="form-input" />
                </div>
                <div>
                  <label class="form-label">Ngày kết thúc</label>
                  <input v-model="form.endDate" type="date" class="form-input" />
                </div>
              </div>
              <div>
                <label class="form-label">Số người tối đa</label>
                <input v-model.number="form.maxParticipants" type="number" class="form-input" min="2" placeholder="Để trống nếu không giới hạn" />
              </div>
              <div class="flex justify-end gap-3 pt-4">
                <button type="button" @click="showCreateModal = false" class="btn-secondary">Hủy</button>
                <button type="submit" class="btn-primary" :disabled="saving">
                  {{ saving ? 'Đang tạo...' : 'Tạo thử thách' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
  </MainLayout>
</template>

<script setup>
import { challengesApi } from '@/api/challenges.api'
import MainLayout from '@/components/layout/MainLayout.vue'
import { formatCurrency, formatDate, getStatusLabel } from '@/utils/format'
import { computed, onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()

const loading = ref(true)
const challenges = ref([])
const selectedStatus = ref('')
const showCreateModal = ref(false)
const saving = ref(false)

const statusFilters = [
  { label: 'Tất cả', value: '' },
  { label: 'Đang mở', value: 'Open' },
  { label: 'Đang diễn ra', value: 'Ongoing' },
  { label: 'Đã kết thúc', value: 'Finished' }
]

const form = reactive({
  title: '',
  description: '',
  entryFee: 0,
  prizePool: 0,
  startDate: '',
  endDate: '',
  maxParticipants: null
})

const filteredChallenges = computed(() => {
  if (!selectedStatus.value) return challenges.value
  return challenges.value.filter(c => c.status === selectedStatus.value)
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

const openCreateModal = () => {
  Object.assign(form, {
    title: '',
    description: '',
    entryFee: 0,
    prizePool: 0,
    startDate: '',
    endDate: '',
    maxParticipants: null
  })
  showCreateModal.value = true
}

const submitChallenge = async () => {
  saving.value = true
  try {
    await challengesApi.create(form)
    toast.success('Tạo thử thách thành công!')
    showCreateModal.value = false
    fetchChallenges()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo thử thách')
  } finally {
    saving.value = false
  }
}

const joinChallenge = async (challenge) => {
  try {
    await challengesApi.join(challenge.id)
    toast.success('Đã tham gia thử thách!')
    fetchChallenges()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tham gia')
  }
}

const fetchChallenges = async () => {
  loading.value = true
  try {
    const response = await challengesApi.getAll()
    challenges.value = response.data?.data || response.data || []
  } catch (error) {
    toast.error('Không thể tải danh sách thử thách')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchChallenges()
})
</script>
