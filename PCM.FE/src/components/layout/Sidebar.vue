<template>
  <aside
    :class="[
      'fixed inset-y-0 left-0 z-50 w-64 bg-white border-r border-gray-200 transform transition-transform duration-300 ease-in-out lg:translate-x-0',
      open ? 'translate-x-0' : '-translate-x-full'
    ]"
  >
    <!-- Logo -->
    <div class="flex items-center justify-between h-16 px-6 border-b border-gray-200">
      <router-link to="/dashboard" class="flex items-center gap-3">
        <div class="w-10 h-10 bg-primary-600 rounded-xl flex items-center justify-center">
          <span class="text-white font-bold text-xl">P</span>
        </div>
        <span class="text-xl font-bold text-gray-900">PCM</span>
      </router-link>
      <button @click="$emit('close')" class="lg:hidden p-2 rounded-lg hover:bg-gray-100">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
    
    <!-- Navigation -->
    <nav class="p-4 space-y-1 overflow-y-auto h-[calc(100vh-4rem)]">
      <template v-for="item in filteredMenuItems" :key="item.path">
        <!-- Menu group header -->
        <div v-if="item.header" class="pt-4 pb-2 px-3">
          <span class="text-xs font-semibold text-gray-400 uppercase tracking-wider">
            {{ item.header }}
          </span>
        </div>
        
        <!-- Menu item -->
        <router-link
          v-else
          :to="item.path"
          :class="[
            'flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium transition-all duration-200',
            isActive(item.path)
              ? 'bg-primary-50 text-primary-700'
              : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
          ]"
          @click="$emit('close')"
        >
          <component :is="item.icon" class="w-5 h-5" />
          <span>{{ item.label }}</span>
          <span 
            v-if="item.badge" 
            class="ml-auto px-2 py-0.5 text-xs rounded-full bg-primary-100 text-primary-700"
          >
            {{ item.badge }}
          </span>
        </router-link>
      </template>
    </nav>
  </aside>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth.store'
import { computed, h } from 'vue'
import { useRoute } from 'vue-router'

defineProps({
  open: Boolean
})

defineEmits(['close'])

const route = useRoute()
const authStore = useAuthStore()

// Icon components
const HomeIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' })
])

const UsersIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' })
])

const NewspaperIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z' })
])

const CourtIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4' })
])

const CalendarIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' })
])

const TrophyIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z' })
])

const GamepadIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z' }),
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M21 12a9 9 0 11-18 0 9 9 0 0118 0z' })
])

const CurrencyIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z' })
])

const TagIcon = () => h('svg', { fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z' })
])

// Menu items
const menuItems = [
  { header: 'Tổng quan' },
  { path: '/dashboard', label: 'Dashboard', icon: HomeIcon },
  
  { header: 'Quản lý' },
  { path: '/members', label: 'Thành viên', icon: UsersIcon },
  { path: '/news', label: 'Tin tức', icon: NewspaperIcon },
  { path: '/courts', label: 'Sân', icon: CourtIcon, roles: ['Admin'] },
  { path: '/bookings', label: 'Đặt sân', icon: CalendarIcon },
  
  { header: 'Hoạt động' },
  { path: '/challenges', label: 'Thử thách', icon: TrophyIcon },
  { path: '/matches', label: 'Trận đấu', icon: GamepadIcon, roles: ['Admin', 'Referee'] },
  
  { header: 'Tài chính', roles: ['Admin', 'Treasurer'] },
  { path: '/transactions', label: 'Thu chi', icon: CurrencyIcon, roles: ['Admin', 'Treasurer'] },
  { path: '/transaction-categories', label: 'Danh mục', icon: TagIcon, roles: ['Admin'] },
]

// Filter menu items based on user role
const filteredMenuItems = computed(() => {
  return menuItems.filter(item => {
    if (!item.roles) return true
    return item.roles.some(role => authStore.hasRole(role))
  })
})

const isActive = (path) => {
  return route.path === path || route.path.startsWith(path + '/')
}
</script>
