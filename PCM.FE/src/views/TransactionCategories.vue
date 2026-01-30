<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Danh mục thu chi</h1>
          <p class="page-subtitle">Quản lý các danh mục giao dịch</p>
        </div>
        <button @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm danh mục
        </button>
      </div>
      
      <!-- Categories List -->
      <div class="card">
        <div class="card-body p-0">
          <div v-if="loading" class="p-8">
            <div v-for="i in 5" :key="i" class="flex items-center justify-between py-4 border-b border-gray-100">
              <div class="skeleton h-5 w-48"></div>
              <div class="skeleton h-8 w-20"></div>
            </div>
          </div>
          
          <div v-else-if="categories.length === 0" class="p-12 text-center text-gray-500">
            Chưa có danh mục nào
          </div>
          
          <div v-else class="divide-y divide-gray-100">
            <div 
              v-for="category in categories" 
              :key="category.id"
              class="flex items-center justify-between p-4 hover:bg-gray-50"
            >
              <div class="flex items-center gap-4">
                <div :class="[
                  'w-10 h-10 rounded-full flex items-center justify-center',
                  category.type === 'Income' ? 'bg-green-100' : 'bg-red-100'
                ]">
                  <span :class="category.type === 'Income' ? 'text-green-600' : 'text-red-600'" class="text-lg">
                    {{ category.type === 'Income' ? '📈' : '📉' }}
                  </span>
                </div>
                <div>
                  <p class="font-medium text-gray-900">{{ category.name }}</p>
                  <p class="text-sm text-gray-500">
                    {{ category.type === 'Income' ? 'Thu' : 'Chi' }}
                    {{ category.description ? ' • ' + category.description : '' }}
                  </p>
                </div>
              </div>
              <div class="flex gap-2">
                <button @click="editCategory(category)" class="p-2 hover:bg-gray-100 rounded-lg">
                  <svg class="w-5 h-5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button @click="confirmDelete(category)" class="p-2 hover:bg-gray-100 rounded-lg">
                  <svg class="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Create/Edit Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-md w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">{{ isEditing ? 'Chỉnh sửa' : 'Thêm' }} danh mục</h3>
            </div>
            <form @submit.prevent="submitCategory" class="p-6 space-y-4">
              <div>
                <label class="form-label">Tên danh mục *</label>
                <input v-model="form.name" type="text" class="form-input" required placeholder="VD: Phí hội viên" />
              </div>
              <div>
                <label class="form-label">Loại *</label>
                <div class="flex gap-4">
                  <label class="flex items-center gap-2 cursor-pointer">
                    <input type="radio" v-model="form.type" value="Income" class="form-radio" />
                    <span>Thu</span>
                  </label>
                  <label class="flex items-center gap-2 cursor-pointer">
                    <input type="radio" v-model="form.type" value="Expense" class="form-radio" />
                    <span>Chi</span>
                  </label>
                </div>
              </div>
              <div>
                <label class="form-label">Mô tả</label>
                <textarea v-model="form.description" class="form-input" rows="2" placeholder="Mô tả danh mục..."></textarea>
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
    
    <ConfirmDialog
      :show="showDeleteConfirm"
      title="Xác nhận xóa"
      message="Bạn có chắc chắn muốn xóa danh mục này? Các giao dịch sử dụng danh mục này có thể bị ảnh hưởng."
      @confirm="deleteCategory"
      @cancel="showDeleteConfirm = false"
    />
  </MainLayout>
</template>

<script setup>
import { transactionsApi } from '@/api/transactions.api'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()

const loading = ref(true)
const categories = ref([])
const showModal = ref(false)
const showDeleteConfirm = ref(false)
const isEditing = ref(false)
const saving = ref(false)
const selectedCategory = ref(null)

const form = reactive({
  name: '',
  type: 'Income',
  description: ''
})

const fetchCategories = async () => {
  loading.value = true
  try {
    const response = await transactionsApi.getCategories()
    categories.value = response.data?.data || response.data || []
  } catch (error) {
    toast.error('Không thể tải danh mục')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  isEditing.value = false
  Object.assign(form, {
    name: '',
    type: 'Income',
    description: ''
  })
  showModal.value = true
}

const editCategory = (category) => {
  isEditing.value = true
  selectedCategory.value = category
  Object.assign(form, {
    name: category.name,
    type: category.type,
    description: category.description || ''
  })
  showModal.value = true
}

const submitCategory = async () => {
  saving.value = true
  try {
    if (isEditing.value) {
      await transactionsApi.updateCategory(selectedCategory.value.id, form)
      toast.success('Cập nhật thành công!')
    } else {
      await transactionsApi.createCategory(form)
      toast.success('Thêm danh mục thành công!')
    }
    showModal.value = false
    fetchCategories()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể lưu danh mục')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (category) => {
  selectedCategory.value = category
  showDeleteConfirm.value = true
}

const deleteCategory = async () => {
  try {
    await transactionsApi.deleteCategory(selectedCategory.value.id)
    toast.success('Đã xóa danh mục')
    showDeleteConfirm.value = false
    fetchCategories()
  } catch (error) {
    toast.error('Không thể xóa danh mục')
  }
}

onMounted(() => {
  fetchCategories()
})
</script>
