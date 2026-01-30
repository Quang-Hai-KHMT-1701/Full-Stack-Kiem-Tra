<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Quản lý trận đấu</h1>
          <p class="page-subtitle">Theo dõi và nhập kết quả trận đấu</p>
        </div>
        <button v-if="canCreateMatch" @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Tạo trận đấu
        </button>
      </div>
      
      <!-- Filters -->
      <div class="card p-4">
        <div class="flex flex-wrap gap-4">
          <div>
            <label class="form-label">Trạng thái</label>
            <select v-model="filters.status" class="form-input">
              <option value="">Tất cả</option>
              <option value="Scheduled">Đã lên lịch</option>
              <option value="Playing">Đang diễn ra</option>
              <option value="Finished">Đã kết thúc</option>
            </select>
          </div>
          <div>
            <label class="form-label">Từ ngày</label>
            <input type="date" v-model="filters.fromDate" class="form-input" />
          </div>
          <div>
            <label class="form-label">Đến ngày</label>
            <input type="date" v-model="filters.toDate" class="form-input" />
          </div>
        </div>
      </div>
      
      <!-- Matches List -->
      <div v-if="loading" class="space-y-4">
        <div v-for="i in 5" :key="i" class="card p-4">
          <div class="flex items-center justify-between">
            <div class="space-y-2">
              <div class="skeleton h-5 w-48"></div>
              <div class="skeleton h-4 w-32"></div>
            </div>
            <div class="skeleton h-8 w-20"></div>
          </div>
        </div>
      </div>
      
      <div v-else-if="filteredMatches.length === 0" class="card p-12 text-center">
        <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h3 class="text-lg font-medium text-gray-900">Chưa có trận đấu nào</h3>
      </div>
      
      <div v-else class="space-y-4">
        <div 
          v-for="match in filteredMatches" 
          :key="match.id"
          class="card p-6 hover:shadow-lg transition-shadow cursor-pointer"
          @click="openMatchDetail(match)"
        >
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-6">
              <!-- Team 1 -->
              <div class="text-center">
                <p class="font-semibold text-gray-900">{{ getTeamName(match, 1) }}</p>
                <p v-if="match.status === 'Finished'" class="text-2xl font-bold text-primary-600">
                  {{ match.team1Score || 0 }}
                </p>
              </div>
              
              <div class="text-gray-400 text-xl font-bold">VS</div>
              
              <!-- Team 2 -->
              <div class="text-center">
                <p class="font-semibold text-gray-900">{{ getTeamName(match, 2) }}</p>
                <p v-if="match.status === 'Finished'" class="text-2xl font-bold text-primary-600">
                  {{ match.team2Score || 0 }}
                </p>
              </div>
            </div>
            
            <div class="text-right">
              <span :class="getStatusBadgeClass(match.status)">
                {{ getStatusLabel(match.status) }}
              </span>
              <p class="text-sm text-gray-500 mt-1">{{ formatDate(match.matchDate) }}</p>
              <p class="text-sm text-gray-500">{{ match.court?.name }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Create Match Modal (Step by Step) -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showCreateModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div class="px-6 py-4 border-b border-gray-100 sticky top-0 bg-white">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold">Tạo trận đấu mới</h3>
                <div class="flex items-center gap-2">
                  <span 
                    v-for="step in 4" 
                    :key="step"
                    :class="[
                      'w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium',
                      currentStep >= step 
                        ? 'bg-primary-600 text-white' 
                        : 'bg-gray-200 text-gray-600'
                    ]"
                  >
                    {{ step }}
                  </span>
                </div>
              </div>
            </div>
            
            <form @submit.prevent="submitMatch" class="p-6 space-y-6">
              <!-- Step 1: Match Info -->
              <div v-if="currentStep === 1" class="space-y-4">
                <h4 class="font-semibold text-gray-700">Bước 1: Thông tin trận đấu</h4>
                <div>
                  <label class="form-label">Thể thức *</label>
                  <select v-model="matchForm.format" class="form-input" required>
                    <option value="Singles">Đơn</option>
                    <option value="Doubles">Đôi</option>
                  </select>
                </div>
                <div>
                  <label class="form-label">Sân *</label>
                  <select v-model="matchForm.courtId" class="form-input" required>
                    <option value="">Chọn sân</option>
                    <option v-for="court in courts" :key="court.id" :value="court.id">
                      {{ court.name }}
                    </option>
                  </select>
                </div>
                <div>
                  <label class="form-label">Ngày giờ *</label>
                  <input v-model="matchForm.matchDate" type="datetime-local" class="form-input" required />
                </div>
              </div>
              
              <!-- Step 2: Players -->
              <div v-if="currentStep === 2" class="space-y-4">
                <h4 class="font-semibold text-gray-700">
                  Bước 2: Chọn người chơi ({{ matchForm.format === 'Singles' ? 'Đơn' : 'Đôi' }})
                </h4>
                
                <div class="grid md:grid-cols-2 gap-6">
                  <!-- Team 1 -->
                  <div class="p-4 border border-gray-200 rounded-xl">
                    <h5 class="font-medium mb-3 text-primary-700">Đội 1</h5>
                    <div class="space-y-3">
                      <div>
                        <label class="form-label">Người chơi 1 *</label>
                        <select v-model="matchForm.team1Player1Id" class="form-input" required>
                          <option value="">Chọn</option>
                          <option 
                            v-for="member in availableMembers([])" 
                            :key="member.id" 
                            :value="member.id"
                          >
                            {{ member.fullName }}
                          </option>
                        </select>
                      </div>
                      <div v-if="matchForm.format === 'Doubles'">
                        <label class="form-label">Người chơi 2 *</label>
                        <select v-model="matchForm.team1Player2Id" class="form-input" :required="matchForm.format === 'Doubles'">
                          <option value="">Chọn</option>
                          <option 
                            v-for="member in availableMembers([matchForm.team1Player1Id])" 
                            :key="member.id" 
                            :value="member.id"
                          >
                            {{ member.fullName }}
                          </option>
                        </select>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Team 2 -->
                  <div class="p-4 border border-gray-200 rounded-xl">
                    <h5 class="font-medium mb-3 text-secondary-700">Đội 2</h5>
                    <div class="space-y-3">
                      <div>
                        <label class="form-label">Người chơi 1 *</label>
                        <select v-model="matchForm.team2Player1Id" class="form-input" required>
                          <option value="">Chọn</option>
                          <option 
                            v-for="member in availableMembers([matchForm.team1Player1Id, matchForm.team1Player2Id])" 
                            :key="member.id" 
                            :value="member.id"
                          >
                            {{ member.fullName }}
                          </option>
                        </select>
                      </div>
                      <div v-if="matchForm.format === 'Doubles'">
                        <label class="form-label">Người chơi 2 *</label>
                        <select v-model="matchForm.team2Player2Id" class="form-input" :required="matchForm.format === 'Doubles'">
                          <option value="">Chọn</option>
                          <option 
                            v-for="member in availableMembers([matchForm.team1Player1Id, matchForm.team1Player2Id, matchForm.team2Player1Id])" 
                            :key="member.id" 
                            :value="member.id"
                          >
                            {{ member.fullName }}
                          </option>
                        </select>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Step 3: Result (Optional) -->
              <div v-if="currentStep === 3" class="space-y-4">
                <h4 class="font-semibold text-gray-700">Bước 3: Kết quả (tùy chọn)</h4>
                <p class="text-sm text-gray-500">Bỏ qua nếu trận đấu chưa diễn ra</p>
                
                <div class="grid md:grid-cols-2 gap-6">
                  <div>
                    <label class="form-label">Điểm Đội 1</label>
                    <input v-model.number="matchForm.team1Score" type="number" class="form-input" min="0" />
                  </div>
                  <div>
                    <label class="form-label">Điểm Đội 2</label>
                    <input v-model.number="matchForm.team2Score" type="number" class="form-input" min="0" />
                  </div>
                </div>
                
                <div>
                  <label class="form-label">Ghi chú</label>
                  <textarea v-model="matchForm.notes" class="form-input" rows="2"></textarea>
                </div>
              </div>
              
              <!-- Step 4: Link to Challenge -->
              <div v-if="currentStep === 4" class="space-y-4">
                <h4 class="font-semibold text-gray-700">Bước 4: Liên kết thử thách (tùy chọn)</h4>
                <p class="text-sm text-gray-500">Gắn trận đấu này với một thử thách đang diễn ra</p>
                
                <div>
                  <label class="form-label">Thử thách</label>
                  <select v-model="matchForm.challengeId" class="form-input">
                    <option value="">Không liên kết</option>
                    <option 
                      v-for="ch in ongoingChallenges" 
                      :key="ch.id" 
                      :value="ch.id"
                    >
                      {{ ch.title }}
                    </option>
                  </select>
                </div>
              </div>
              
              <!-- Navigation -->
              <div class="flex justify-between pt-4 border-t border-gray-100">
                <button 
                  v-if="currentStep > 1"
                  type="button" 
                  @click="currentStep--" 
                  class="btn-secondary"
                >
                  ← Quay lại
                </button>
                <div v-else></div>
                
                <div class="flex gap-3">
                  <button type="button" @click="showCreateModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button 
                    v-if="currentStep < 4"
                    type="button" 
                    @click="nextStep" 
                    class="btn-primary"
                    :disabled="!canProceed"
                  >
                    Tiếp tục →
                  </button>
                  <button 
                    v-else
                    type="submit" 
                    class="btn-primary"
                    :disabled="saving"
                  >
                    {{ saving ? 'Đang tạo...' : 'Tạo trận đấu' }}
                  </button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
    
    <!-- Match Detail Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showDetailModal && selectedMatch" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-lg w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">Chi tiết trận đấu</h3>
            </div>
            <div class="p-6 space-y-4">
              <div class="flex items-center justify-center gap-8 py-6">
                <div class="text-center">
                  <p class="font-semibold text-lg">{{ getTeamName(selectedMatch, 1) }}</p>
                  <p class="text-4xl font-bold text-primary-600">{{ selectedMatch.team1Score || 0 }}</p>
                </div>
                <span class="text-2xl text-gray-400">VS</span>
                <div class="text-center">
                  <p class="font-semibold text-lg">{{ getTeamName(selectedMatch, 2) }}</p>
                  <p class="text-4xl font-bold text-primary-600">{{ selectedMatch.team2Score || 0 }}</p>
                </div>
              </div>
              
              <div class="space-y-3 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-500">Trạng thái</span>
                  <span :class="getStatusBadgeClass(selectedMatch.status)">{{ getStatusLabel(selectedMatch.status) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Sân</span>
                  <span>{{ selectedMatch.court?.name }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Thời gian</span>
                  <span>{{ formatDate(selectedMatch.matchDate) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Thể thức</span>
                  <span>{{ selectedMatch.format === 'Singles' ? 'Đơn' : 'Đôi' }}</span>
                </div>
              </div>
              
              <div v-if="canEditMatch" class="flex justify-end gap-3 pt-4">
                <button @click="showDetailModal = false" class="btn-secondary">Đóng</button>
                <button @click="updateMatchResult" class="btn-primary">Cập nhật kết quả</button>
              </div>
              <div v-else class="flex justify-end pt-4">
                <button @click="showDetailModal = false" class="btn-secondary">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </MainLayout>
</template>

<script setup>
import { challengesApi } from '@/api/challenges.api'
import { matchesApi } from '@/api/matches.api'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { useCommonStore } from '@/stores/common.store'
import { formatDate, getStatusLabel } from '@/utils/format'
import { computed, onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const authStore = useAuthStore()
const commonStore = useCommonStore()

const loading = ref(true)
const matches = ref([])
const courts = ref([])
const members = ref([])
const ongoingChallenges = ref([])
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const selectedMatch = ref(null)
const currentStep = ref(1)
const saving = ref(false)

const filters = reactive({
  status: '',
  fromDate: '',
  toDate: ''
})

const matchForm = reactive({
  format: 'Singles',
  courtId: '',
  matchDate: '',
  team1Player1Id: '',
  team1Player2Id: '',
  team2Player1Id: '',
  team2Player2Id: '',
  team1Score: null,
  team2Score: null,
  challengeId: '',
  notes: ''
})

const canCreateMatch = computed(() => {
  return authStore.hasRole('Admin') || authStore.hasRole('Referee')
})

const canEditMatch = computed(() => {
  return authStore.hasRole('Admin') || authStore.hasRole('Referee')
})

const filteredMatches = computed(() => {
  let result = [...matches.value]
  
  if (filters.status) {
    result = result.filter(m => m.status === filters.status)
  }
  
  if (filters.fromDate) {
    result = result.filter(m => m.matchDate >= filters.fromDate)
  }
  
  if (filters.toDate) {
    result = result.filter(m => m.matchDate <= filters.toDate)
  }
  
  return result
})

const canProceed = computed(() => {
  if (currentStep.value === 1) {
    return matchForm.courtId && matchForm.matchDate
  }
  if (currentStep.value === 2) {
    if (matchForm.format === 'Singles') {
      return matchForm.team1Player1Id && matchForm.team2Player1Id
    }
    return matchForm.team1Player1Id && matchForm.team1Player2Id && 
           matchForm.team2Player1Id && matchForm.team2Player2Id
  }
  return true
})

const getStatusBadgeClass = (status) => {
  const classes = {
    'Scheduled': 'badge-info',
    'Playing': 'badge-warning',
    'Finished': 'badge-success',
    'Cancelled': 'badge-danger'
  }
  return classes[status] || 'badge-info'
}

const getTeamName = (match, team) => {
  if (!match.players || match.players.length === 0) {
    return team === 1 ? 'Đội 1' : 'Đội 2'
  }
  
  const teamPlayers = match.players.filter(p => p.team === team)
  if (teamPlayers.length === 0) return `Đội ${team}`
  
  return teamPlayers.map(p => p.member?.fullName || 'Unknown').join(' & ')
}

const availableMembers = (excludeIds) => {
  return members.value.filter(m => !excludeIds.includes(m.id))
}

const openCreateModal = () => {
  currentStep.value = 1
  Object.assign(matchForm, {
    format: 'Singles',
    courtId: '',
    matchDate: '',
    team1Player1Id: '',
    team1Player2Id: '',
    team2Player1Id: '',
    team2Player2Id: '',
    team1Score: null,
    team2Score: null,
    challengeId: '',
    notes: ''
  })
  showCreateModal.value = true
}

const openMatchDetail = (match) => {
  selectedMatch.value = match
  showDetailModal.value = true
}

const nextStep = () => {
  if (canProceed.value && currentStep.value < 4) {
    currentStep.value++
  }
}

const submitMatch = async () => {
  saving.value = true
  try {
    await matchesApi.create(matchForm)
    toast.success('Tạo trận đấu thành công!')
    showCreateModal.value = false
    fetchMatches()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo trận đấu')
  } finally {
    saving.value = false
  }
}

const updateMatchResult = () => {
  // TODO: Implement update result modal
  toast.info('Chức năng đang phát triển')
}

const fetchMatches = async () => {
  loading.value = true
  try {
    const response = await matchesApi.getAll()
    matches.value = response.data?.data || response.data || []
  } catch (error) {
    toast.error('Không thể tải danh sách trận đấu')
  } finally {
    loading.value = false
  }
}

const fetchChallenges = async () => {
  try {
    const response = await challengesApi.getAll({ status: 'Ongoing' })
    ongoingChallenges.value = response.data?.data || response.data || []
  } catch (error) {
    console.error('Error fetching challenges:', error)
  }
}

onMounted(async () => {
  courts.value = await commonStore.fetchCourts()
  members.value = await commonStore.fetchMembers()
  await fetchMatches()
  await fetchChallenges()
})
</script>
