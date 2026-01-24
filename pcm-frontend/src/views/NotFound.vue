<template>
  <div class="min-h-screen bg-gray-100 flex items-center justify-center p-4">
    <div class="text-center">
      <div class="mb-8">
        <h1 class="text-9xl font-bold text-primary-600">404</h1>
        <div class="relative">
          <div class="absolute -top-10 left-1/2 transform -translate-x-1/2">
            <svg class="w-24 h-24 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
      </div>
      
      <h2 class="text-2xl font-bold text-gray-900 mb-4">Oops! Trang không tồn tại</h2>
      <p class="text-gray-600 mb-8 max-w-md mx-auto">
        Trang bạn đang tìm kiếm có thể đã bị xóa, đổi tên hoặc tạm thời không khả dụng.
      </p>
      
      <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
        <router-link to="/" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
          </svg>
          Về trang chủ
        </router-link>
        <button @click="$router.back()" class="btn-secondary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Quay lại
        </button>
      </div>
      
      <div class="mt-12 pt-8 border-t border-gray-200 max-w-md mx-auto">
        <p class="text-sm text-gray-500 mb-4">Có thể bạn muốn:</p>
        <div class="flex flex-wrap justify-center gap-3 text-sm">
          <router-link to="/dashboard" class="text-primary-600 hover:underline">Dashboard</router-link>
          <span class="text-gray-300">|</span>
          <router-link to="/members" class="text-primary-600 hover:underline">Hội viên</router-link>
          <span class="text-gray-300">|</span>
          <router-link to="/bookings" class="text-primary-600 hover:underline">Đặt sân</router-link>
          <span class="text-gray-300">|</span>
          <router-link to="/challenges" class="text-primary-600 hover:underline">Thử thách</router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// No script needed for this simple component
</script>
