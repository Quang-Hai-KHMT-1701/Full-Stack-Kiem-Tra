<template>
  <MainLayout>
    <div class="max-w-2xl mx-auto space-y-6">
      <h1 class="page-title">Hồ sơ cá nhân</h1>
      
      <div v-if="loading" class="card p-6">
        <div class="flex items-center gap-6">
          <div class="skeleton w-24 h-24 rounded-full"></div>
          <div class="flex-1 space-y-3">
            <div class="skeleton h-6 w-48"></div>
            <div class="skeleton h-4 w-32"></div>
          </div>
        </div>
      </div>
      
      <template v-else>
        <!-- Profile Card -->
        <div class="card p-6">
          <div class="flex flex-col sm:flex-row items-center gap-6">
            <div class="relative">
              <div class="w-24 h-24 bg-primary-100 rounded-full flex items-center justify-center">
                <span class="text-3xl font-bold text-primary-600">
                  {{ getInitials(user.fullName) }}
                </span>
              </div>
              <button 
                class="absolute bottom-0 right-0 w-8 h-8 bg-white rounded-full shadow-md flex items-center justify-center hover:bg-gray-50"
                @click="changeAvatar"
              >
                <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </button>
            </div>
            <div class="text-center sm:text-left">
              <h2 class="text-xl font-bold text-gray-900">{{ user.fullName }}</h2>
              <p class="text-gray-500">{{ user.email }}</p>
              <div class="flex flex-wrap justify-center sm:justify-start gap-2 mt-2">
                <span v-for="role in user.roles" :key="role" class="badge-primary">
                  {{ getRoleLabel(role) }}
                </span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Edit Profile Form -->
        <div class="card">
          <div class="card-header">
            <h3 class="font-semibold">Thông tin cá nhân</h3>
          </div>
          <form @submit.prevent="updateProfile" class="card-body space-y-4">
            <div class="grid sm:grid-cols-2 gap-4">
              <div>
                <label class="form-label">Họ và tên *</label>
                <input v-model="form.fullName" type="text" class="form-input" required />
              </div>
              <div>
                <label class="form-label">Số điện thoại</label>
                <input v-model="form.phoneNumber" type="tel" class="form-input" />
              </div>
            </div>
            <div>
              <label class="form-label">Email</label>
              <input :value="user.email" type="email" class="form-input bg-gray-50" disabled />
              <p class="text-xs text-gray-500 mt-1">Email không thể thay đổi</p>
            </div>
            <div>
              <label class="form-label">Ngày sinh</label>
              <input v-model="form.birthDate" type="date" class="form-input" />
            </div>
            <div>
              <label class="form-label">Giới thiệu bản thân</label>
              <textarea v-model="form.bio" class="form-input" rows="3" placeholder="Một vài thông tin về bạn..."></textarea>
            </div>
            <div class="flex justify-end pt-4">
              <button type="submit" class="btn-primary" :disabled="saving">
                {{ saving ? 'Đang lưu...' : 'Cập nhật thông tin' }}
              </button>
            </div>
          </form>
        </div>
        
        <!-- Change Password -->
        <div class="card">
          <div class="card-header">
            <h3 class="font-semibold">Đổi mật khẩu</h3>
          </div>
          <form @submit.prevent="changePassword" class="card-body space-y-4">
            <div>
              <label class="form-label">Mật khẩu hiện tại *</label>
              <input v-model="passwordForm.currentPassword" type="password" class="form-input" required />
            </div>
            <div>
              <label class="form-label">Mật khẩu mới *</label>
              <input v-model="passwordForm.newPassword" type="password" class="form-input" required minlength="6" />
            </div>
            <div>
              <label class="form-label">Xác nhận mật khẩu mới *</label>
              <input v-model="passwordForm.confirmPassword" type="password" class="form-input" required />
            </div>
            <div class="flex justify-end pt-4">
              <button type="submit" class="btn-primary" :disabled="changingPassword">
                {{ changingPassword ? 'Đang đổi...' : 'Đổi mật khẩu' }}
              </button>
            </div>
          </form>
        </div>
        
        <!-- Stats -->
        <div class="card">
          <div class="card-header">
            <h3 class="font-semibold">Thống kê hoạt động</h3>
          </div>
          <div class="card-body">
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
              <div class="text-center p-4 bg-gray-50 rounded-xl">
                <p class="text-2xl font-bold text-primary-600">{{ stats.matchesPlayed || 0 }}</p>
                <p class="text-sm text-gray-500">Trận đã đấu</p>
              </div>
              <div class="text-center p-4 bg-gray-50 rounded-xl">
                <p class="text-2xl font-bold text-green-600">{{ stats.wins || 0 }}</p>
                <p class="text-sm text-gray-500">Thắng</p>
              </div>
              <div class="text-center p-4 bg-gray-50 rounded-xl">
                <p class="text-2xl font-bold text-orange-600">{{ stats.challengesJoined || 0 }}</p>
                <p class="text-sm text-gray-500">Thử thách</p>
              </div>
              <div class="text-center p-4 bg-gray-50 rounded-xl">
                <p class="text-2xl font-bold text-blue-600">{{ stats.bookingsCount || 0 }}</p>
                <p class="text-sm text-gray-500">Đặt sân</p>
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>
  </MainLayout>
</template>

<script setup>
import { authApi } from '@/api/auth.api'
import { membersApi } from '@/api/members.api'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import dayjs from 'dayjs'
import { computed, onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const saving = ref(false)
const changingPassword = ref(false)

const user = computed(() => authStore.user || {})

const form = reactive({
  fullName: '',
  phoneNumber: '',
  birthDate: '',
  bio: ''
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const stats = reactive({
  matchesPlayed: 0,
  wins: 0,
  challengesJoined: 0,
  bookingsCount: 0
})

const getInitials = (name) => {
  if (!name) return '?'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

const getRoleLabel = (role) => {
  const labels = {
    'Admin': 'Quản trị viên',
    'Member': 'Hội viên',
    'Referee': 'Trọng tài',
    'Treasurer': 'Thủ quỹ'
  }
  return labels[role] || role
}

const fetchProfile = async () => {
  loading.value = true
  try {
    const response = await membersApi.getProfile()
    const profile = response.data
    
    Object.assign(form, {
      fullName: profile.fullName || '',
      phoneNumber: profile.phoneNumber || '',
      birthDate: profile.birthDate ? dayjs(profile.birthDate).format('YYYY-MM-DD') : '',
      bio: profile.bio || ''
    })
    
    // Update stats
    if (profile.stats) {
      Object.assign(stats, profile.stats)
    }
  } catch (error) {
    console.error('Error fetching profile:', error)
  } finally {
    loading.value = false
  }
}

const updateProfile = async () => {
  saving.value = true
  try {
    await membersApi.updateProfile(form)
    toast.success('Cập nhật thông tin thành công!')
    // Update local user data
    if (authStore.user) {
      authStore.user.fullName = form.fullName
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể cập nhật')
  } finally {
    saving.value = false
  }
}

const changePassword = async () => {
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    toast.error('Mật khẩu xác nhận không khớp')
    return
  }
  
  if (passwordForm.newPassword.length < 6) {
    toast.error('Mật khẩu mới phải có ít nhất 6 ký tự')
    return
  }
  
  changingPassword.value = true
  try {
    await authApi.changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword
    })
    toast.success('Đổi mật khẩu thành công!')
    // Clear form
    Object.assign(passwordForm, {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    })
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể đổi mật khẩu')
  } finally {
    changingPassword.value = false
  }
}

const changeAvatar = () => {
  toast.info('Chức năng đang phát triển')
}

onMounted(() => {
  fetchProfile()
})
</script>
