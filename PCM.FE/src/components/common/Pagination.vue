<template>
  <div class="flex items-center justify-between px-4 py-3 bg-white border-t border-gray-200">
    <!-- Info -->
    <div class="text-sm text-gray-600">
      Hiển thị {{ startItem }} - {{ endItem }} / {{ total }} kết quả
    </div>
    
    <!-- Pagination buttons -->
    <div class="flex items-center gap-1">
      <!-- First -->
      <button 
        @click="goToPage(1)"
        :disabled="currentPage === 1"
        class="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Trang đầu"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
        </svg>
      </button>
      
      <!-- Previous -->
      <button 
        @click="goToPage(currentPage - 1)"
        :disabled="currentPage === 1"
        class="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Trang trước"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      
      <!-- Page numbers -->
      <template v-for="page in visiblePages" :key="page">
        <span v-if="page === '...'" class="px-2 text-gray-400">...</span>
        <button 
          v-else
          @click="goToPage(page)"
          :class="[
            'px-3 py-1 rounded-lg text-sm font-medium transition-colors',
            currentPage === page 
              ? 'bg-primary-600 text-white' 
              : 'hover:bg-gray-100 text-gray-700'
          ]"
        >
          {{ page }}
        </button>
      </template>
      
      <!-- Next -->
      <button 
        @click="goToPage(currentPage + 1)"
        :disabled="currentPage === totalPages"
        class="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Trang sau"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
      
      <!-- Last -->
      <button 
        @click="goToPage(totalPages)"
        :disabled="currentPage === totalPages"
        class="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Trang cuối"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 5l7 7-7 7M5 5l7 7-7 7" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  currentPage: {
    type: Number,
    required: true
  },
  totalPages: {
    type: Number,
    required: true
  },
  total: {
    type: Number,
    required: true
  },
  pageSize: {
    type: Number,
    default: 10
  }
})

const emit = defineEmits(['page-change'])

const startItem = computed(() => {
  return (props.currentPage - 1) * props.pageSize + 1
})

const endItem = computed(() => {
  return Math.min(props.currentPage * props.pageSize, props.total)
})

const visiblePages = computed(() => {
  const pages = []
  const total = props.totalPages
  const current = props.currentPage
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 3) {
      pages.push(1, 2, 3, 4, '...', total)
    } else if (current >= total - 2) {
      pages.push(1, '...', total - 3, total - 2, total - 1, total)
    } else {
      pages.push(1, '...', current - 1, current, current + 1, '...', total)
    }
  }
  
  return pages
})

const goToPage = (page) => {
  if (page >= 1 && page <= props.totalPages && page !== props.currentPage) {
    emit('page-change', page)
  }
}
</script>
