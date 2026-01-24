<template>
  <div class="min-h-screen bg-gradient-to-br from-primary-600 to-primary-800 flex items-center justify-center p-4">
    <div class="w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-16 h-16 bg-white rounded-2xl shadow-lg mb-4">
          <span class="text-3xl font-bold text-primary-600">P</span>
        </div>
        <h1 class="text-3xl font-bold text-white">PCM</h1>
        <p class="text-primary-200 mt-1">Đăng ký tài khoản mới</p>
      </div>
      
      <!-- Register Card -->
      <div class="bg-white rounded-2xl shadow-xl p-8">
        <h2 class="text-2xl font-bold text-gray-900 text-center mb-6">Đăng ký</h2>
        
        <form @submit.prevent="handleRegister" class="space-y-4">
          <!-- Full Name -->
          <div>
            <label for="fullName" class="form-label">Họ và tên</label>
            <input
              id="fullName"
              v-model="form.fullName"
              type="text"
              class="form-input"
              :class="{ 'form-input-error': errors.fullName }"
              placeholder="Nguyễn Văn A"
              required
            />
            <p v-if="errors.fullName" class="form-error">{{ errors.fullName }}</p>
          </div>
          
          <!-- Email -->
          <div>
            <label for="email" class="form-label">Email</label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              class="form-input"
              :class="{ 'form-input-error': errors.email }"
              placeholder="your@email.com"
              required
            />
            <p v-if="errors.email" class="form-error">{{ errors.email }}</p>
          </div>
          
          <!-- Phone -->
          <div>
            <label for="phoneNumber" class="form-label">Số điện thoại</label>
            <input
              id="phoneNumber"
              v-model="form.phoneNumber"
              type="tel"
              class="form-input"
              :class="{ 'form-input-error': errors.phoneNumber }"
              placeholder="0901234567"
            />
            <p v-if="errors.phoneNumber" class="form-error">{{ errors.phoneNumber }}</p>
          </div>
          
          <!-- Password -->
          <div>
            <label for="password" class="form-label">Mật khẩu</label>
            <input
              id="password"
              v-model="form.password"
              type="password"
              class="form-input"
              :class="{ 'form-input-error': errors.password }"
              placeholder="••••••••"
              required
            />
            <p v-if="errors.password" class="form-error">{{ errors.password }}</p>
          </div>
          
          <!-- Confirm Password -->
          <div>
            <label for="confirmPassword" class="form-label">Xác nhận mật khẩu</label>
            <input
              id="confirmPassword"
              v-model="form.confirmPassword"
              type="password"
              class="form-input"
              :class="{ 'form-input-error': errors.confirmPassword }"
              placeholder="••••••••"
              required
            />
            <p v-if="errors.confirmPassword" class="form-error">{{ errors.confirmPassword }}</p>
          </div>
          
          <!-- Submit button -->
          <button
            type="submit"
            :disabled="loading"
            class="btn-primary w-full py-3 mt-6"
          >
            <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ loading ? 'Đang đăng ký...' : 'Đăng ký' }}
          </button>
        </form>
        
        <!-- Login link -->
        <p class="text-center text-gray-600 mt-6">
          Đã có tài khoản?
          <router-link to="/login" class="text-primary-600 hover:text-primary-700 font-medium">
            Đăng nhập
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { authApi } from '@/api/auth.api'
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const router = useRouter()
const toast = useToast()

const loading = ref(false)

const form = reactive({
  fullName: '',
  email: '',
  phoneNumber: '',
  password: '',
  confirmPassword: ''
})

const errors = reactive({
  fullName: '',
  email: '',
  phoneNumber: '',
  password: '',
  confirmPassword: ''
})

const validateForm = () => {
  let isValid = true
  Object.keys(errors).forEach(key => errors[key] = '')
  
  if (!form.fullName) {
    errors.fullName = 'Vui lòng nhập họ tên'
    isValid = false
  }
  
  if (!form.email) {
    errors.email = 'Vui lòng nhập email'
    isValid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Email không hợp lệ'
    isValid = false
  }
  
  if (!form.password) {
    errors.password = 'Vui lòng nhập mật khẩu'
    isValid = false
  } else if (form.password.length < 6) {
    errors.password = 'Mật khẩu phải có ít nhất 6 ký tự'
    isValid = false
  }
  
  if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Mật khẩu xác nhận không khớp'
    isValid = false
  }
  
  return isValid
}

const handleRegister = async () => {
  if (!validateForm()) return
  
  loading.value = true
  
  try {
    await authApi.register({
      fullName: form.fullName,
      email: form.email,
      phoneNumber: form.phoneNumber,
      password: form.password
    })
    
    toast.success('Đăng ký thành công! Vui lòng đăng nhập.')
    router.push('/login')
  } catch (error) {
    const message = error.response?.data?.message || 'Đăng ký thất bại'
    toast.error(message)
  } finally {
    loading.value = false
  }
}
</script>
