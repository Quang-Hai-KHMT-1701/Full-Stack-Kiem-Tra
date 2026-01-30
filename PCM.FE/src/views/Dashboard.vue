<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Welcome header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 class="page-title">Xin chào, {{ authStore.user?.fullName || 'bạn' }}! 👋</h1>
          <p class="page-subtitle">Chào mừng bạn quay trở lại với PCM</p>
        </div>
        <div class="mt-4 sm:mt-0">
          <span class="text-sm text-gray-500">{{ currentDate }}</span>
        </div>
      </div>
      
      <!-- Stats cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatsCard
          title="Số dư quỹ"
          :value="formatCurrency(summary.balance || 0)"
          :trend="summary.balance >= 0 ? 'up' : 'down'"
          :trendValue="summary.balance >= 0 ? 'Ổn định' : 'Cần chú ý'"
          icon="currency"
          :iconBg="summary.balance >= 0 ? 'bg-green-100' : 'bg-red-100'"
          :iconColor="summary.balance >= 0 ? 'text-green-600' : 'text-red-600'"
          v-if="authStore.hasRole('Admin') || authStore.hasRole('Treasurer')"
        />
        <StatsCard
          title="Thành viên"
          :value="stats.totalMembers || 0"
          trend="up"
          trendValue="+5 tuần này"
          icon="users"
          iconBg="bg-blue-100"
          iconColor="text-blue-600"
        />
        <StatsCard
          title="Thử thách đang mở"
          :value="stats.openChallenges || 0"
          icon="trophy"
          iconBg="bg-yellow-100"
          iconColor="text-yellow-600"
        />
        <StatsCard
          title="Đặt sân hôm nay"
          :value="stats.todayBookings || 0"
          icon="calendar"
          iconBg="bg-purple-100"
          iconColor="text-purple-600"
        />
      </div>
      
      <!-- Warning for negative balance -->
      <div 
        v-if="(authStore.isAdmin || authStore.isTreasurer) && summary.balance < 0"
        class="bg-red-50 border border-red-200 rounded-xl p-4 flex items-start gap-3"
      >
        <svg class="w-6 h-6 text-red-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
        <div>
          <h3 class="font-semibold text-red-800">Cảnh báo: Quỹ đang âm!</h3>
          <p class="text-red-600 text-sm mt-1">
            Số dư hiện tại: {{ formatCurrency(summary.balance) }}. Vui lòng kiểm tra và bổ sung quỹ.
          </p>
        </div>
      </div>
      
      <!-- Main content grid -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Left column - 2 cols -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Pinned News -->
          <div class="card">
            <div class="card-header flex items-center justify-between">
              <h2 class="font-semibold text-gray-900">📌 Tin tức ghim</h2>
              <router-link to="/news" class="text-primary-600 hover:text-primary-700 text-sm font-medium">
                Xem tất cả →
              </router-link>
            </div>
            <div class="card-body">
              <div v-if="loadingNews" class="space-y-4">
                <div v-for="i in 3" :key="i" class="skeleton h-20 rounded-lg"></div>
              </div>
              <div v-else-if="pinnedNews.length === 0" class="text-center py-8 text-gray-500">
                Chưa có tin tức ghim
              </div>
              <div v-else class="space-y-4">
                <div 
                  v-for="news in pinnedNews" 
                  :key="news.id"
                  class="p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer"
                  @click="$router.push(`/news/${news.id}`)"
                >
                  <h3 class="font-medium text-gray-900">{{ news.title }}</h3>
                  <p class="text-sm text-gray-500 mt-1 line-clamp-2">{{ news.content }}</p>
                  <span class="text-xs text-gray-400 mt-2 block">{{ formatDate(news.createdAt) }}</span>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Open Challenges -->
          <div class="card">
            <div class="card-header flex items-center justify-between">
              <h2 class="font-semibold text-gray-900">🏆 Thử thách đang mở</h2>
              <router-link to="/challenges" class="text-primary-600 hover:text-primary-700 text-sm font-medium">
                Xem tất cả →
              </router-link>
            </div>
            <div class="card-body">
              <div v-if="loadingChallenges" class="grid sm:grid-cols-2 gap-4">
                <div v-for="i in 4" :key="i" class="skeleton h-32 rounded-lg"></div>
              </div>
              <div v-else-if="openChallenges.length === 0" class="text-center py-8 text-gray-500">
                Không có thử thách nào đang mở
              </div>
              <div v-else class="grid sm:grid-cols-2 gap-4">
                <div 
                  v-for="challenge in openChallenges.slice(0, 4)" 
                  :key="challenge.id"
                  class="p-4 border border-gray-200 rounded-xl hover:border-primary-300 hover:shadow-md transition-all cursor-pointer"
                  @click="$router.push(`/challenges/${challenge.id}`)"
                >
                  <div class="flex items-start justify-between">
                    <h3 class="font-medium text-gray-900">{{ challenge.title }}</h3>
                    <span class="badge-success">Open</span>
                  </div>
                  <div class="mt-3 space-y-1 text-sm text-gray-600">
                    <p>💰 Phí: {{ formatCurrency(challenge.entryFee || 0) }}</p>
                    <p>🎁 Giải: {{ formatCurrency(challenge.prizePool || 0) }}</p>
                    <p>👥 {{ challenge.participantCount || 0 }}/{{ challenge.maxParticipants || '∞' }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Right column -->
        <div class="space-y-6">
          <!-- Top Ranking -->
          <TopRanking :members="topMembers" :loading="loadingMembers" />
          
          <!-- Quick Actions -->
          <div class="card">
            <div class="card-header">
              <h2 class="font-semibold text-gray-900">⚡ Hành động nhanh</h2>
            </div>
            <div class="card-body space-y-2">
              <router-link 
                to="/bookings" 
                class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                  <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <span class="font-medium text-gray-900">Đặt sân</span>
              </router-link>
              
              <router-link 
                to="/challenges" 
                class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <div class="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center">
                  <svg class="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                  </svg>
                </div>
                <span class="font-medium text-gray-900">Tham gia thử thách</span>
              </router-link>
              
              <router-link 
                v-if="authStore.isReferee"
                to="/matches" 
                class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                  <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <span class="font-medium text-gray-900">Ghi nhận trận đấu</span>
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </MainLayout>
</template>

<script setup>
import { challengesApi } from '@/api/challenges.api'
import { dashboardApi } from '@/api/dashboard.api'
import { membersApi } from '@/api/members.api'
import { newsApi } from '@/api/news.api'
import StatsCard from '@/components/dashboard/StatsCard.vue'
import TopRanking from '@/components/dashboard/TopRanking.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { formatCurrency, formatDate } from '@/utils/format'
import dayjs from 'dayjs'
import { computed, onMounted, ref } from 'vue'

const authStore = useAuthStore()

// State
const loadingNews = ref(true)
const loadingChallenges = ref(true)
const loadingMembers = ref(true)
const loadingSummary = ref(true)

const pinnedNews = ref([])
const openChallenges = ref([])
const topMembers = ref([])
const summary = ref({ income: 0, expense: 0, balance: 0 })
const stats = ref({
  totalMembers: 0,
  openChallenges: 0,
  todayBookings: 0
})

const currentDate = computed(() => {
  return dayjs().format('dddd, DD/MM/YYYY')
})

// Fetch data
const fetchPinnedNews = async () => {
  loadingNews.value = true
  try {
    const response = await dashboardApi.getPinnedNews()
    pinnedNews.value = response.data || []
  } catch (error) {
    console.error('Error fetching news:', error)
    // Fallback to old API
    try {
      const response = await newsApi.getPinned()
      pinnedNews.value = response.data?.data || response.data || []
    } catch (e) {
      console.error('Fallback error:', e)
    }
  } finally {
    loadingNews.value = false
  }
}

const fetchOpenChallenges = async () => {
  loadingChallenges.value = true
  try {
    const response = await dashboardApi.getUpcomingChallenges()
    openChallenges.value = response.data || []
    stats.value.openChallenges = openChallenges.value.length
  } catch (error) {
    console.error('Error fetching challenges:', error)
    // Fallback to old API
    try {
      const response = await challengesApi.getAll({ status: 'Open' })
      openChallenges.value = response.data?.data || response.data || []
      stats.value.openChallenges = openChallenges.value.length
    } catch (e) {
      console.error('Fallback error:', e)
    }
  } finally {
    loadingChallenges.value = false
  }
}

const fetchTopMembers = async () => {
  loadingMembers.value = true
  try {
    const response = await dashboardApi.getTopMembers(5)
    topMembers.value = response.data || []
    
    // Also get total members from stats
    if (authStore.hasRole('Admin') || authStore.hasRole('Treasurer')) {
      const statsResponse = await dashboardApi.getStats()
      if (statsResponse.data) {
        stats.value.totalMembers = statsResponse.data.members?.total || 0
        stats.value.openChallenges = statsResponse.data.challenges?.open || 0
        stats.value.todayBookings = statsResponse.data.bookings?.today || 0
      }
    } else {
      // Fallback for non-admin
      const allResponse = await membersApi.getAll()
      const allMembers = allResponse.data?.data || allResponse.data || []
      stats.value.totalMembers = allMembers.length || allResponse.data?.total || 0
    }
  } catch (error) {
    console.error('Error fetching members:', error)
    // Fallback to old API
    try {
      const response = await membersApi.getTopRanking(5)
      topMembers.value = response.data?.data || response.data || []
    } catch (e) {
      console.error('Fallback error:', e)
    }
  } finally {
    loadingMembers.value = false
  }
}

const fetchSummary = async () => {
  if (!authStore.hasRole('Admin') && !authStore.hasRole('Treasurer')) {
    loadingSummary.value = false
    return
  }
  
  loadingSummary.value = true
  try {
    const response = await dashboardApi.getStats()
    if (response.data?.finance) {
      summary.value = {
        income: response.data.finance.totalIncome || 0,
        expense: response.data.finance.totalExpense || 0,
        balance: response.data.finance.balance || 0
      }
    }
  } catch (error) {
    console.error('Error fetching summary:', error)
  } finally {
    loadingSummary.value = false
  }
}

onMounted(() => {
  fetchPinnedNews()
  fetchOpenChallenges()
  fetchTopMembers()
  fetchSummary()
})
</script>
