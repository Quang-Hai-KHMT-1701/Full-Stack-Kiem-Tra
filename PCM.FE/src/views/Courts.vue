<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Quản lý sân</h1>
          <p class="page-subtitle">Danh sách các sân trong câu lạc bộ</p>
        </div>
        <button @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm sân mới
        </button>
      </div>
      
      <!-- Courts grid -->
      <div v-if="loading" class="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="i in 6" :key="i" class="card">
          <div class="skeleton h-40 rounded-t-xl"></div>
          <div class="p-4 space-y-2">
            <div class="skeleton h-5 w-3/4"></div>
            <div class="skeleton h-4 w-1/2"></div>
          </div>
        </div>
      </div>
      
      <div v-else-if="courts.length === 0" class="card p-12 text-center">
        <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
        </svg>
        <h3 class="text-lg font-medium text-gray-900">Chưa có sân nào</h3>
        <p class="text-gray-500 mt-1">Bắt đầu bằng việc thêm sân mới</p>
        <button @click="openCreateModal" class="btn-primary mt-4">Thêm sân đầu tiên</button>
      </div>
      
      <div v-else class="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="court in courts" :key="court.id" class="card hover:shadow-lg transition-shadow">
          <div class="h-40 bg-gradient-to-br from-green-400 to-green-600 flex items-center justify-center">
            <span class="text-white text-4xl font-bold">{{ court.name }}</span>
          </div>
          <div class="p-4">
            <div class="flex items-start justify-between">
              <div>
                <h3 class="font-semibold text-gray-900">{{ court.name }}</h3>
                <p class="text-sm text-gray-500 mt-1">{{ court.location || 'Chưa có vị trí' }}</p>
              </div>
              <span :class="court.isActive ? 'badge-success' : 'badge-danger'">
                {{ court.isActive ? 'Hoạt động' : 'Tạm dừng' }}
              </span>
            </div>
            <p class="text-sm text-gray-600 mt-2 line-clamp-2">{{ court.description || 'Không có mô tả' }}</p>
            <div class="flex items-center justify-between mt-4 pt-4 border-t border-gray-100">
              <span class="font-semibold text-primary-600">{{ formatCurrency(court.pricePerHour) }}/giờ</span>
              <div class="flex gap-2">
                <button @click="editCourt(court)" class="p-2 text-blue-600 hover:bg-blue-50 rounded-lg">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button @click="confirmDelete(court)" class="p-2 text-red-600 hover:bg-red-50 rounded-lg">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
              <h3 class="text-lg font-semibold">{{ isEditing ? 'Chỉnh sửa sân' : 'Thêm sân mới' }}</h3>
            </div>
            <form @submit.prevent="saveCourt" class="p-6 space-y-4">
              <div>
                <label class="form-label">Tên sân *</label>
                <input v-model="form.name" type="text" class="form-input" required placeholder="VD: Sân A" />
              </div>
              <div>
                <label class="form-label">Vị trí</label>
                <input v-model="form.location" type="text" class="form-input" placeholder="VD: Tầng 1, Tòa nhà A" />
              </div>
              <div>
                <label class="form-label">Mô tả</label>
                <textarea v-model="form.description" class="form-input" rows="3" placeholder="Mô tả về sân..."></textarea>
              </div>
              <div>
                <label class="form-label">Giá thuê/giờ (VNĐ) *</label>
                <input v-model.number="form.pricePerHour" type="number" class="form-input" required min="0" />
              </div>
              <div class="flex items-center">
                <input type="checkbox" v-model="form.isActive" id="isActive" class="rounded border-gray-300 text-primary-600 focus:ring-primary-500" />
                <label for="isActive" class="ml-2 text-sm text-gray-700">Đang hoạt động</label>
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
    
    <!-- Confirm Delete Dialog -->
    <ConfirmDialog
      v-model="showDeleteConfirm"
      title="Xóa sân"
      :message="`Bạn có chắc chắn muốn xóa sân '${selectedCourt?.name}'? Hành động này không thể hoàn tác.`"
      confirm-text="Xóa"
      type="danger"
      :loading="deleting"
      @confirm="deleteCourt"
    />
  </MainLayout>
</template>

<script setup>
import { courtsApi } from '@/api/courts.api'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { formatCurrency } from '@/utils/format'
import { onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()

const loading = ref(true)
const courts = ref([])
const showModal = ref(false)
const showDeleteConfirm = ref(false)
const saving = ref(false)
const deleting = ref(false)
const isEditing = ref(false)
const selectedCourt = ref(null)

const form = reactive({
  name: '',
  location: '',
  description: '',
  pricePerHour: 0,
  isActive: true
})

const resetForm = () => {
  form.name = ''
  form.location = ''
  form.description = ''
  form.pricePerHour = 0
  form.isActive = true
}

const openCreateModal = () => {
  isEditing.value = false
  resetForm()
  showModal.value = true
}

const editCourt = (court) => {
  isEditing.value = true
  selectedCourt.value = court
  Object.assign(form, {
    name: court.name,
    location: court.location || '',
    description: court.description || '',
    pricePerHour: court.pricePerHour,
    isActive: court.isActive
  })
  showModal.value = true
}

const saveCourt = async () => {
  saving.value = true
  try {
    if (isEditing.value) {
      await courtsApi.update(selectedCourt.value.id, form)
      toast.success('Cập nhật sân thành công')
    } else {
      await courtsApi.create(form)
      toast.success('Thêm sân mới thành công')
    }
    showModal.value = false
    fetchCourts()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Có lỗi xảy ra')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (court) => {
  selectedCourt.value = court
  showDeleteConfirm.value = true
}

const deleteCourt = async () => {
  deleting.value = true
  try {
    await courtsApi.delete(selectedCourt.value.id)
    toast.success('Xóa sân thành công')
    showDeleteConfirm.value = false
    fetchCourts()
  } catch (error) {
    toast.error('Không thể xóa sân')
  } finally {
    deleting.value = false
  }
}

const fetchCourts = async () => {
  loading.value = true
  try {
    const response = await courtsApi.getAll()
    courts.value = response.data?.data || response.data || []
  } catch (error) {
    toast.error('Không thể tải danh sách sân')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchCourts()
})
</script>
