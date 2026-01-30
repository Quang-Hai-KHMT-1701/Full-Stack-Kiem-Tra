<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Quản lý thành viên</h1>
          <p class="page-subtitle">Danh sách tất cả thành viên câu lạc bộ</p>
        </div>
        <div class="flex items-center gap-3">
          <div class="relative">
            <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm kiếm thành viên..."
              class="form-input pl-10 w-64"
              @input="debouncedSearch"
            />
          </div>
          <!-- Filter dropdown -->
          <select v-model="filterStatus" class="form-input" @change="fetchMembers">
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đang hoạt động</option>
            <option value="false">Đã vô hiệu</option>
          </select>
          <!-- Add button for Admin -->
          <button v-if="authStore.isAdmin" @click="openCreateModal" class="btn-primary">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Thêm thành viên
          </button>
        </div>
      </div>
      
      <!-- Members table -->
      <div class="card">
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>Thành viên</th>
                <th>Email</th>
                <th>Điện thoại</th>
                <th>Rank Level</th>
                <th>Trạng thái</th>
                <th class="text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="text-center py-8">
                  <LoadingSpinner text="Đang tải..." />
                </td>
              </tr>
              <tr v-else-if="members.length === 0">
                <td colspan="6" class="text-center py-8 text-gray-500">
                  Không tìm thấy thành viên nào
                </td>
              </tr>
              <tr v-else v-for="member in members" :key="member.id" :class="{ 'opacity-50': !member.isActive }">
                <td>
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center">
                      <span class="text-primary-700 font-medium">{{ getInitials(member.fullName) }}</span>
                    </div>
                    <div>
                      <p class="font-medium text-gray-900">{{ member.fullName }}</p>
                      <p class="text-sm text-gray-500">ID: {{ member.id }}</p>
                    </div>
                  </div>
                </td>
                <td>{{ member.email }}</td>
                <td>{{ member.phoneNumber || '-' }}</td>
                <td>
                  <div class="flex items-center gap-2">
                    <span class="font-semibold text-primary-600">{{ member.rankLevel?.toFixed(1) || '0.0' }}</span>
                    <button 
                      v-if="authStore.isAdmin"
                      @click="openRankModal(member)"
                      class="p-1 text-gray-400 hover:text-primary-600"
                      title="Cập nhật rank"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                      </svg>
                    </button>
                  </div>
                </td>
                <td>
                  <button 
                    v-if="authStore.isAdmin"
                    @click="toggleStatus(member)"
                    :class="member.isActive ? 'badge-success cursor-pointer hover:bg-green-200' : 'badge-error cursor-pointer hover:bg-red-200'"
                    :title="member.isActive ? 'Click để vô hiệu hóa' : 'Click để kích hoạt'"
                  >
                    {{ member.isActive ? 'Hoạt động' : 'Vô hiệu' }}
                  </button>
                  <span v-else :class="member.isActive ? 'badge-success' : 'badge-error'">
                    {{ member.isActive ? 'Hoạt động' : 'Vô hiệu' }}
                  </span>
                </td>
                <td class="text-right">
                  <div class="flex items-center justify-end gap-2">
                    <button 
                      @click="viewMember(member)"
                      class="p-2 text-gray-500 hover:text-primary-600 hover:bg-primary-50 rounded-lg"
                      title="Xem chi tiết"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                      </svg>
                    </button>
                    <button 
                      v-if="canEdit(member)"
                      @click="editMember(member)"
                      class="p-2 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg"
                      title="Sửa"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                    </button>
                    <button 
                      v-if="authStore.isAdmin"
                      @click="confirmDelete(member)"
                      class="p-2 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-lg"
                      title="Xóa"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <!-- Pagination -->
        <Pagination
          v-if="totalPages > 1"
          :current-page="currentPage"
          :total-pages="totalPages"
          :total="total"
          :page-size="pageSize"
          @page-change="handlePageChange"
        />
      </div>
    </div>
    
    <!-- Edit Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showEditModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-md w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">Chỉnh sửa thông tin</h3>
            </div>
            <form @submit.prevent="saveEdit" class="p-6 space-y-4">
              <div>
                <label class="form-label">Họ và tên</label>
                <input v-model="editForm.fullName" type="text" class="form-input" required />
              </div>
              <div>
                <label class="form-label">Email</label>
                <input v-model="editForm.email" type="email" class="form-input" required />
              </div>
              <div>
                <label class="form-label">Số điện thoại</label>
                <input v-model="editForm.phoneNumber" type="tel" class="form-input" />
              </div>
              <div class="flex justify-end gap-3 pt-4">
                <button type="button" @click="showEditModal = false" class="btn-secondary">Hủy</button>
                <button type="submit" class="btn-primary" :disabled="saving">
                  {{ saving ? 'Đang lưu...' : 'Lưu thay đổi' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Rank Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showRankModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-sm w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">Cập nhật Rank</h3>
              <p class="text-sm text-gray-500">{{ selectedMember?.fullName }}</p>
            </div>
            <form @submit.prevent="saveRank" class="p-6 space-y-4">
              <div>
                <label class="form-label">Rank Level (0-10)</label>
                <input v-model.number="rankForm.rankLevel" type="number" step="0.1" min="0" max="10" class="form-input" required />
              </div>
              <div class="flex justify-end gap-3 pt-4">
                <button type="button" @click="showRankModal = false" class="btn-secondary">Hủy</button>
                <button type="submit" class="btn-primary" :disabled="saving">
                  {{ saving ? 'Đang lưu...' : 'Cập nhật' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Delete Confirm Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showDeleteModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-sm w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold text-red-600">Xác nhận xóa</h3>
            </div>
            <div class="p-6">
              <p class="text-gray-600">
                Bạn có chắc muốn xóa thành viên <strong>{{ selectedMember?.fullName }}</strong>?
              </p>
              <p class="text-sm text-gray-500 mt-2">
                Hành động này sẽ vô hiệu hóa tài khoản (soft delete).
              </p>
              <div class="flex justify-end gap-3 pt-6">
                <button type="button" @click="showDeleteModal = false" class="btn-secondary">Hủy</button>
                <button @click="deleteMember" class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700" :disabled="saving">
                  {{ saving ? 'Đang xóa...' : 'Xóa' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </MainLayout>
</template>

<script setup>
import { membersApi } from '@/api/members.api'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Pagination from '@/components/common/Pagination.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const members = ref([])
const searchQuery = ref('')
const filterStatus = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

const showEditModal = ref(false)
const showRankModal = ref(false)
const showDeleteModal = ref(false)
const saving = ref(false)
const selectedMember = ref(null)

const editForm = reactive({
  fullName: '',
  email: '',
  phoneNumber: ''
})

const rankForm = reactive({
  rankLevel: 0
})

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

const canEdit = (member) => {
  return authStore.isAdmin || authStore.user?.id === member.id
}

const getInitials = (name) => {
  if (!name) return '?'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

// Debounce search
let searchTimeout = null
const debouncedSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    currentPage.value = 1
    fetchMembers()
  }, 500)
}

const fetchMembers = async () => {
  loading.value = true
  try {
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value
    }
    if (searchQuery.value) params.search = searchQuery.value
    if (filterStatus.value) params.isActive = filterStatus.value

    const response = await membersApi.getAll(params)
    members.value = response.data?.data || response.data || []
    total.value = response.data?.total || members.value.length
  } catch (error) {
    toast.error('Không thể tải danh sách thành viên')
  } finally {
    loading.value = false
  }
}

const viewMember = (member) => {
  router.push(`/members/${member.id}`)
}

const editMember = (member) => {
  selectedMember.value = member
  editForm.fullName = member.fullName
  editForm.email = member.email
  editForm.phoneNumber = member.phoneNumber || ''
  showEditModal.value = true
}

const saveEdit = async () => {
  saving.value = true
  try {
    await membersApi.update(selectedMember.value.id, editForm)
    toast.success('Cập nhật thành công')
    showEditModal.value = false
    fetchMembers()
  } catch (error) {
    toast.error('Không thể cập nhật thông tin')
  } finally {
    saving.value = false
  }
}

const openRankModal = (member) => {
  selectedMember.value = member
  rankForm.rankLevel = member.rankLevel || 0
  showRankModal.value = true
}

const saveRank = async () => {
  saving.value = true
  try {
    await membersApi.updateRank(selectedMember.value.id, rankForm.rankLevel)
    toast.success('Cập nhật rank thành công')
    showRankModal.value = false
    fetchMembers()
  } catch (error) {
    toast.error('Không thể cập nhật rank')
  } finally {
    saving.value = false
  }
}

const toggleStatus = async (member) => {
  if (!confirm(`Bạn có chắc muốn ${member.isActive ? 'vô hiệu hóa' : 'kích hoạt'} thành viên ${member.fullName}?`)) {
    return
  }
  try {
    await membersApi.updateStatus(member.id, !member.isActive)
    toast.success(`Đã ${member.isActive ? 'vô hiệu hóa' : 'kích hoạt'} thành viên`)
    fetchMembers()
  } catch (error) {
    toast.error('Không thể cập nhật trạng thái')
  }
}

const confirmDelete = (member) => {
  selectedMember.value = member
  showDeleteModal.value = true
}

const deleteMember = async () => {
  saving.value = true
  try {
    await membersApi.delete(selectedMember.value.id)
    toast.success('Đã xóa thành viên')
    showDeleteModal.value = false
    fetchMembers()
  } catch (error) {
    toast.error('Không thể xóa thành viên')
  } finally {
    saving.value = false
  }
}

const handlePageChange = (page) => {
  currentPage.value = page
  fetchMembers()
}

onMounted(() => {
  fetchMembers()
})
</script>
