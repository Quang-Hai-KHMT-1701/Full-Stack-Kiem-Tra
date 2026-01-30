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
            />
          </div>
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
                <th>Điểm XH</th>
                <th>Vai trò</th>
                <th class="text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="text-center py-8">
                  <LoadingSpinner text="Đang tải..." />
                </td>
              </tr>
              <tr v-else-if="filteredMembers.length === 0">
                <td colspan="6" class="text-center py-8 text-gray-500">
                  Không tìm thấy thành viên nào
                </td>
              </tr>
              <tr v-else v-for="member in filteredMembers" :key="member.id">
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
                  <span class="font-semibold text-primary-600">{{ member.rankingPoints || 0 }}</span>
                </td>
                <td>
                  <div class="flex flex-wrap gap-1">
                    <span 
                      v-for="role in member.roles || ['Member']" 
                      :key="role"
                      :class="getRoleBadgeClass(role)"
                    >
                      {{ getRoleLabel(role) }}
                    </span>
                  </div>
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
  </MainLayout>
</template>

<script setup>
import { membersApi } from '@/api/members.api'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Pagination from '@/components/common/Pagination.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { getRoleBadgeClass, getRoleLabel } from '@/utils/roles'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const members = ref([])
const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

const showEditModal = ref(false)
const saving = ref(false)
const selectedMember = ref(null)
const editForm = reactive({
  fullName: '',
  email: '',
  phoneNumber: ''
})

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

const filteredMembers = computed(() => {
  if (!searchQuery.value) return members.value
  const query = searchQuery.value.toLowerCase()
  return members.value.filter(m => 
    m.fullName?.toLowerCase().includes(query) ||
    m.email?.toLowerCase().includes(query)
  )
})

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

const fetchMembers = async () => {
  loading.value = true
  try {
    const response = await membersApi.getAll({
      page: currentPage.value,
      pageSize: pageSize.value
    })
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

const handlePageChange = (page) => {
  currentPage.value = page
  fetchMembers()
}

onMounted(() => {
  fetchMembers()
})
</script>
