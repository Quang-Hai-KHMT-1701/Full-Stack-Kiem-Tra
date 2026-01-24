<template>
  <div class="card">
    <div class="card-header flex items-center justify-between">
      <h2 class="font-semibold text-gray-900">🏅 BXH Top 5</h2>
      <router-link to="/members" class="text-primary-600 hover:text-primary-700 text-sm font-medium">
        Xem tất cả →
      </router-link>
    </div>
    <div class="card-body">
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 5" :key="i" class="flex items-center gap-3">
          <div class="skeleton w-8 h-8 rounded-full"></div>
          <div class="flex-1">
            <div class="skeleton h-4 w-3/4 mb-1"></div>
            <div class="skeleton h-3 w-1/2"></div>
          </div>
        </div>
      </div>
      
      <div v-else-if="members.length === 0" class="text-center py-6 text-gray-500">
        Chưa có dữ liệu xếp hạng
      </div>
      
      <div v-else class="space-y-3">
        <div 
          v-for="(member, index) in members" 
          :key="member.id"
          class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <!-- Rank badge -->
          <div 
            class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold"
            :class="getRankClass(index)"
          >
            {{ index + 1 }}
          </div>
          
          <!-- Avatar -->
          <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center">
            <span class="text-primary-700 font-medium">
              {{ getInitials(member.fullName) }}
            </span>
          </div>
          
          <!-- Info -->
          <div class="flex-1 min-w-0">
            <p class="font-medium text-gray-900 truncate">{{ member.fullName }}</p>
            <p class="text-sm text-gray-500">{{ member.rankingPoints || 0 }} điểm</p>
          </div>
          
          <!-- Medal for top 3 -->
          <span v-if="index < 3" class="text-xl">
            {{ ['🥇', '🥈', '🥉'][index] }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  members: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const getRankClass = (index) => {
  if (index === 0) return 'bg-yellow-400 text-yellow-900'
  if (index === 1) return 'bg-gray-300 text-gray-700'
  if (index === 2) return 'bg-orange-300 text-orange-800'
  return 'bg-gray-100 text-gray-600'
}

const getInitials = (name) => {
  if (!name) return '?'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}
</script>
