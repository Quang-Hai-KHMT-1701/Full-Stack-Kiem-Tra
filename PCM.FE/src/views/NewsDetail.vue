<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Back button -->
      <button @click="$router.back()" class="flex items-center gap-2 text-gray-600 hover:text-gray-900">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Quay lại
      </button>
      
      <div v-if="loading" class="flex justify-center py-12">
        <LoadingSpinner size="lg" text="Đang tải..." />
      </div>
      
      <div v-else-if="!newsItem" class="text-center py-12">
        <p class="text-gray-500">Không tìm thấy bài viết</p>
      </div>
      
      <article v-else class="max-w-3xl mx-auto">
        <!-- Header Image -->
        <div v-if="newsItem.imageUrl" class="rounded-xl overflow-hidden mb-6">
          <img :src="newsItem.imageUrl" :alt="newsItem.title" class="w-full h-64 md:h-80 object-cover" />
        </div>
        
        <!-- Title & Meta -->
        <div class="mb-6">
          <div class="flex items-center gap-2 mb-3">
            <span v-if="newsItem.isPinned" class="badge-warning">📌 Ghim</span>
          </div>
          <h1 class="text-3xl font-bold text-gray-900 mb-4">{{ newsItem.title }}</h1>
          <div class="flex items-center gap-4 text-sm text-gray-500">
            <span class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              {{ formatDate(newsItem.publishedAt || newsItem.createdAt) }}
            </span>
            <span class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              {{ newsItem.author?.fullName || 'Admin' }}
            </span>
            <span v-if="newsItem.viewCount" class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
              {{ newsItem.viewCount }} lượt xem
            </span>
          </div>
        </div>
        
        <!-- Summary -->
        <div v-if="newsItem.summary" class="p-4 bg-primary-50 rounded-xl border-l-4 border-primary-500 mb-6">
          <p class="text-gray-700 italic">{{ newsItem.summary }}</p>
        </div>
        
        <!-- Content -->
        <div class="prose prose-lg max-w-none">
          <div v-html="formattedContent" class="text-gray-700 leading-relaxed"></div>
        </div>
        
        <!-- Admin Actions -->
        <div v-if="authStore.isAdmin" class="flex justify-end gap-3 mt-8 pt-6 border-t border-gray-200">
          <button @click="editNews" class="btn-secondary">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
            Chỉnh sửa
          </button>
          <button @click="confirmDelete" class="btn-danger">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
            Xóa
          </button>
        </div>
      </article>
    </div>
    
    <ConfirmDialog
      :show="showDeleteConfirm"
      title="Xác nhận xóa"
      message="Bạn có chắc chắn muốn xóa bài viết này?"
      @confirm="deleteNews"
      @cancel="showDeleteConfirm = false"
    />
  </MainLayout>
</template>

<script setup>
import { newsApi } from '@/api/news.api'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { formatDate } from '@/utils/format'
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const newsItem = ref(null)
const showDeleteConfirm = ref(false)

const formattedContent = computed(() => {
  if (!newsItem.value?.content) return ''
  // Simple formatting: convert newlines to paragraphs
  return newsItem.value.content
    .split('\n\n')
    .map(p => `<p>${p.replace(/\n/g, '<br>')}</p>`)
    .join('')
})

const fetchNews = async () => {
  loading.value = true
  try {
    const response = await newsApi.getById(route.params.id)
    newsItem.value = response.data
  } catch (error) {
    toast.error('Không thể tải bài viết')
  } finally {
    loading.value = false
  }
}

const editNews = () => {
  router.push(`/news?edit=${newsItem.value.id}`)
}

const confirmDelete = () => {
  showDeleteConfirm.value = true
}

const deleteNews = async () => {
  try {
    await newsApi.delete(newsItem.value.id)
    toast.success('Đã xóa bài viết')
    router.push('/news')
  } catch (error) {
    toast.error('Không thể xóa bài viết')
  }
}

onMounted(() => {
  fetchNews()
})
</script>
