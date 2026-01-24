<template>
  <header class="sticky top-0 z-30 bg-white border-b border-gray-200">
    <div class="flex items-center justify-between h-16 px-4 lg:px-6">
      <!-- Left side -->
      <div class="flex items-center gap-4">
        <!-- Mobile menu button -->
        <button 
          @click="$emit('toggle-sidebar')"
          class="lg:hidden p-2 rounded-lg hover:bg-gray-100"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        
        <!-- Page title / Breadcrumb -->
        <div class="hidden sm:block">
          <h1 class="text-lg font-semibold text-gray-900">{{ pageTitle }}</h1>
        </div>
      </div>
      
      <!-- Right side -->
      <div class="flex items-center gap-3">
        <!-- Notifications -->
        <button class="p-2 rounded-lg hover:bg-gray-100 relative">
          <svg class="w-6 h-6 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <span class="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
        </button>
        
        <!-- User dropdown -->
        <div class="relative" ref="dropdownRef">
          <button 
            @click="dropdownOpen = !dropdownOpen"
            class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-100"
          >
            <div class="w-8 h-8 bg-primary-100 rounded-full flex items-center justify-center">
              <span class="text-primary-700 font-medium text-sm">
                {{ userInitials }}
              </span>
            </div>
            <div class="hidden sm:block text-left">
              <p class="text-sm font-medium text-gray-900">{{ authStore.user?.fullName || 'User' }}</p>
              <p class="text-xs text-gray-500">{{ primaryRole }}</p>
            </div>
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          
          <!-- Dropdown menu -->
          <Transition name="fade">
            <div 
              v-if="dropdownOpen"
              class="absolute right-0 mt-2 w-48 bg-white rounded-xl shadow-lg border border-gray-100 py-2 z-50"
            >
              <router-link 
                to="/profile"
                class="flex items-center gap-3 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                @click="dropdownOpen = false"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                Hồ sơ cá nhân
              </router-link>
              <hr class="my-2 border-gray-100">
              <button 
                @click="handleLogout"
                class="flex items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50 w-full text-left"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Đăng xuất
              </button>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth.store'
import { getRoleLabel } from '@/utils/roles'
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute } from 'vue-router'

defineEmits(['toggle-sidebar'])

const route = useRoute()
const authStore = useAuthStore()
const dropdownOpen = ref(false)
const dropdownRef = ref(null)

// Page titles mapping
const pageTitles = {
  '/dashboard': 'Dashboard',
  '/members': 'Quản lý thành viên',
  '/news': 'Tin tức',
  '/courts': 'Quản lý sân',
  '/bookings': 'Đặt sân',
  '/challenges': 'Thử thách',
  '/matches': 'Trận đấu',
  '/transactions': 'Thu chi',
  '/transaction-categories': 'Danh mục thu chi',
  '/profile': 'Hồ sơ cá nhân'
}

const pageTitle = computed(() => {
  // Find matching route
  for (const path in pageTitles) {
    if (route.path === path || route.path.startsWith(path + '/')) {
      return pageTitles[path]
    }
  }
  return 'PCM'
})

const userInitials = computed(() => {
  const name = authStore.user?.fullName || ''
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
})

const primaryRole = computed(() => {
  const roles = authStore.roles
  if (roles.length === 0) return 'Thành viên'
  return getRoleLabel(roles[0])
})

const handleLogout = () => {
  dropdownOpen.value = false
  authStore.logout()
}

// Close dropdown when clicking outside
const handleClickOutside = (event) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target)) {
    dropdownOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
