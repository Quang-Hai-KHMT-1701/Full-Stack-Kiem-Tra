<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Tin tức & Thông báo</h1>
          <p class="page-subtitle">Cập nhật tin tức mới nhất từ câu lạc bộ</p>
        </div>
        <button v-if="authStore.isAdmin" @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm tin mới
        </button>
      </div>
      
      <!-- Pinned News -->
      <div v-if="pinnedNews.length > 0" class="space-y-4">
        <h2 class="text-lg font-semibold text-gray-900">📌 Tin ghim</h2>
        <div class="grid md:grid-cols-2 gap-4">
          <div 
            v-for="news in pinnedNews" 
            :key="news.id"
            class="card hover:shadow-lg transition-shadow cursor-pointer border-l-4 border-primary-500"
            @click="$router.push(`/news/${news.id}`)"
          >
            <div class="p-5">
              <div class="flex items-start justify-between mb-2">
                <h3 class="font-bold text-lg text-gray-900 line-clamp-2">{{ news.title }}</h3>
                <span class="badge-warning ml-2 shrink-0">Ghim</span>
              </div>
              <p class="text-gray-600 text-sm line-clamp-2 mb-3">{{ news.summary || news.content?.substring(0, 150) }}</p>
              <div class="flex items-center justify-between text-xs text-gray-500">
                <span>{{ formatDate(news.publishedAt || news.createdAt) }}</span>
                <span>{{ news.author?.fullName || 'Admin' }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- All News -->
      <div class="space-y-4">
        <h2 class="text-lg font-semibold text-gray-900">📰 Tất cả tin tức</h2>
        
        <div v-if="loading" class="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="i in 6" :key="i" class="card p-5">
            <div class="skeleton h-6 w-3/4 mb-3"></div>
            <div class="skeleton h-4 w-full mb-2"></div>
            <div class="skeleton h-4 w-2/3 mb-4"></div>
            <div class="skeleton h-3 w-1/3"></div>
          </div>
        </div>
        
        <div v-else-if="news.length === 0" class="card p-12 text-center">
          <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9.5a2 2 0 00-2-2h-2" />
          </svg>
          <h3 class="text-lg font-medium text-gray-900">Chưa có tin tức nào</h3>
        </div>
        
        <div v-else class="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div 
            v-for="item in news" 
            :key="item.id"
            class="card hover:shadow-lg transition-shadow cursor-pointer group"
            @click="$router.push(`/news/${item.id}`)"
          >
            <div v-if="item.imageUrl" class="h-40 bg-gray-200 rounded-t-xl overflow-hidden">
              <img :src="item.imageUrl" :alt="item.title" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
            </div>
            <div class="p-5">
              <h3 class="font-bold text-gray-900 line-clamp-2 mb-2 group-hover:text-primary-600 transition-colors">
                {{ item.title }}
              </h3>
              <p class="text-gray-600 text-sm line-clamp-2 mb-3">
                {{ item.summary || item.content?.substring(0, 120) + '...' }}
              </p>
              <div class="flex items-center justify-between text-xs text-gray-500">
                <span>{{ formatDate(item.publishedAt || item.createdAt) }}</span>
                <div class="flex items-center gap-2">
                  <span v-if="item.viewCount">👁️ {{ item.viewCount }}</span>
                  <button 
                    v-if="authStore.isAdmin"
                    @click.stop="editNews(item)"
                    class="p-1 hover:bg-gray-100 rounded"
                  >
                    <svg class="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <Pagination 
        :current-page="pagination.page" 
        :total-pages="pagination.totalPages"
        @page-change="changePage"
      />
    </div>
    
    <!-- Create/Edit Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div class="px-6 py-4 border-b border-gray-100 sticky top-0 bg-white">
              <h3 class="text-lg font-semibold">{{ isEditing ? 'Chỉnh sửa' : 'Thêm' }} tin tức</h3>
            </div>
            <form @submit.prevent="submitNews" class="p-6 space-y-4">
              <div>
                <label class="form-label">Tiêu đề *</label>
                <input v-model="form.title" type="text" class="form-input" required placeholder="Tiêu đề bài viết" />
              </div>
              <div>
                <label class="form-label">Tóm tắt</label>
                <textarea v-model="form.summary" class="form-input" rows="2" placeholder="Tóm tắt ngắn gọn..."></textarea>
              </div>
              <div>
                <label class="form-label">Nội dung *</label>
                <textarea v-model="form.content" class="form-input" rows="8" required placeholder="Nội dung chi tiết..."></textarea>
              </div>
              <div>
                <label class="form-label">Link ảnh</label>
                <input v-model="form.imageUrl" type="url" class="form-input" placeholder="https://..." />
              </div>
              <div class="flex items-center gap-4">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" v-model="form.isPinned" class="form-checkbox" />
                  <span>Ghim bài viết</span>
                </label>
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" v-model="form.isPublished" class="form-checkbox" />
                  <span>Xuất bản ngay</span>
                </label>
              </div>
              <div class="flex justify-end gap-3 pt-4">
                <button type="button" @click="showModal = false" class="btn-secondary">Hủy</button>
                <button type="submit" class="btn-primary" :disabled="saving">
                  {{ saving ? 'Đang lưu...' : 'Lưu' }}
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
import { newsApi } from '@/api/news.api'
import Pagination from '@/components/common/Pagination.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { formatDate } from '@/utils/format'
import { computed, onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const allNews = ref([])
const showModal = ref(false)
const isEditing = ref(false)
const saving = ref(false)
const selectedNews = ref(null)

const pagination = reactive({
  page: 1,
  pageSize: 12,
  totalPages: 1
})

const form = reactive({
  title: '',
  summary: '',
  content: '',
  imageUrl: '',
  isPinned: false,
  isPublished: true
})

const pinnedNews = computed(() => {
  return allNews.value.filter(n => n.isPinned)
})

const news = computed(() => {
  return allNews.value.filter(n => !n.isPinned)
})

const fetchNews = async () => {
  loading.value = true
  try {
    const response = await newsApi.getAll({
      page: pagination.page,
      pageSize: pagination.pageSize
    })
    const data = response.data?.data || response.data || []
    allNews.value = data.items || data
    pagination.totalPages = data.totalPages || 1
  } catch (error) {
    toast.error('Không thể tải tin tức')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  isEditing.value = false
  Object.assign(form, {
    title: '',
    summary: '',
    content: '',
    imageUrl: '',
    isPinned: false,
    isPublished: true
  })
  showModal.value = true
}

const editNews = (item) => {
  isEditing.value = true
  selectedNews.value = item
  Object.assign(form, {
    title: item.title,
    summary: item.summary || '',
    content: item.content,
    imageUrl: item.imageUrl || '',
    isPinned: item.isPinned || false,
    isPublished: item.isPublished !== false
  })
  showModal.value = true
}

const submitNews = async () => {
  saving.value = true
  try {
    if (isEditing.value) {
      await newsApi.update(selectedNews.value.id, form)
      toast.success('Cập nhật thành công!')
    } else {
      await newsApi.create(form)
      toast.success('Thêm tin tức thành công!')
    }
    showModal.value = false
    fetchNews()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể lưu tin tức')
  } finally {
    saving.value = false
  }
}

const changePage = (page) => {
  pagination.page = page
  fetchNews()
}

onMounted(() => {
  fetchNews()
})
</script>
