<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Đặt sân</h1>
          <p class="page-subtitle">Xem lịch và đặt sân chơi pickleball</p>
        </div>
        <button @click="openBookingModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Đặt sân mới
        </button>
      </div>
      
      <!-- Filters -->
      <div class="card p-4">
        <div class="flex flex-wrap gap-4">
          <div>
            <label class="form-label">Chọn ngày</label>
            <input 
              type="date" 
              v-model="selectedDate" 
              class="form-input"
              @change="fetchBookings"
            />
          </div>
          <div>
            <label class="form-label">Sân</label>
            <select v-model="selectedCourtId" class="form-input" @change="fetchBookings">
              <option value="">Tất cả sân</option>
              <option v-for="court in courts" :key="court.id" :value="court.id">
                {{ court.name }}
              </option>
            </select>
          </div>
        </div>
      </div>
      
      <!-- Calendar View -->
      <div class="card">
        <div class="card-header">
          <h3 class="font-semibold">Lịch đặt sân - {{ formatDate(selectedDate) }}</h3>
        </div>
        <div class="card-body">
          <div v-if="loading" class="space-y-2">
            <div v-for="i in 6" :key="i" class="skeleton h-16 rounded-lg"></div>
          </div>
          
          <div v-else>
            <!-- Time slots grid -->
            <div class="overflow-x-auto">
              <div class="min-w-[600px]">
                <!-- Header - Courts -->
                <div class="grid gap-2" :style="{ gridTemplateColumns: `80px repeat(${displayCourts.length}, 1fr)` }">
                  <div class="p-2 font-semibold text-gray-500 text-sm">Giờ</div>
                  <div 
                    v-for="court in displayCourts" 
                    :key="court.id"
                    class="p-2 font-semibold text-center bg-gray-50 rounded-lg"
                  >
                    {{ court.name }}
                  </div>
                </div>
                
                <!-- Time slots -->
                <div class="mt-2 space-y-2">
                  <div 
                    v-for="slot in timeSlots" 
                    :key="slot"
                    class="grid gap-2"
                    :style="{ gridTemplateColumns: `80px repeat(${displayCourts.length}, 1fr)` }"
                  >
                    <div class="p-2 text-sm font-medium text-gray-600 flex items-center">
                      {{ slot }}
                    </div>
                    <div 
                      v-for="court in displayCourts" 
                      :key="`${court.id}-${slot}`"
                      :class="[
                        'p-2 rounded-lg text-center text-sm cursor-pointer transition-all',
                        getSlotClass(court.id, slot)
                      ]"
                      @click="handleSlotClick(court, slot)"
                    >
                      {{ getSlotText(court.id, slot) }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Legend -->
            <div class="flex items-center gap-6 mt-6 pt-4 border-t border-gray-100">
              <div class="flex items-center gap-2">
                <div class="w-4 h-4 bg-green-100 border border-green-300 rounded"></div>
                <span class="text-sm text-gray-600">Trống</span>
              </div>
              <div class="flex items-center gap-2">
                <div class="w-4 h-4 bg-red-100 border border-red-300 rounded"></div>
                <span class="text-sm text-gray-600">Đã đặt</span>
              </div>
              <div class="flex items-center gap-2">
                <div class="w-4 h-4 bg-yellow-100 border border-yellow-300 rounded"></div>
                <span class="text-sm text-gray-600">Chờ xác nhận</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- My Bookings -->
      <div class="card">
        <div class="card-header">
          <h3 class="font-semibold">Lịch đặt sân của tôi</h3>
        </div>
        <div class="card-body">
          <div v-if="myBookings.length === 0" class="text-center py-8 text-gray-500">
            Bạn chưa có lịch đặt sân nào
          </div>
          <div v-else class="space-y-3">
            <div 
              v-for="booking in myBookings" 
              :key="booking.id"
              class="flex items-center justify-between p-4 bg-gray-50 rounded-xl"
            >
              <div>
                <p class="font-medium">{{ booking.court?.name || 'Sân' }}</p>
                <p class="text-sm text-gray-500">
                  {{ formatDate(booking.date) }} | {{ booking.startTime }} - {{ booking.endTime }}
                </p>
              </div>
              <div class="flex items-center gap-3">
                <span :class="getStatusBadgeClass(booking.status)">
                  {{ getStatusLabel(booking.status) }}
                </span>
                <button 
                  v-if="booking.status === 'Pending'"
                  @click="cancelBooking(booking)"
                  class="text-red-600 hover:text-red-700 text-sm font-medium"
                >
                  Hủy
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Booking Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showBookingModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div class="bg-white rounded-xl shadow-xl max-w-md w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">Đặt sân</h3>
            </div>
            <form @submit.prevent="submitBooking" class="p-6 space-y-4">
              <div>
                <label class="form-label">Sân *</label>
                <select v-model="bookingForm.courtId" class="form-input" required>
                  <option value="">Chọn sân</option>
                  <option v-for="court in courts" :key="court.id" :value="court.id">
                    {{ court.name }} - {{ formatCurrency(court.pricePerHour) }}/giờ
                  </option>
                </select>
              </div>
              <div>
                <label class="form-label">Ngày *</label>
                <input v-model="bookingForm.date" type="date" class="form-input" required :min="today" />
              </div>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="form-label">Bắt đầu *</label>
                  <select v-model="bookingForm.startTime" class="form-input" required>
                    <option v-for="time in timeOptions" :key="time" :value="time">{{ time }}</option>
                  </select>
                </div>
                <div>
                  <label class="form-label">Kết thúc *</label>
                  <select v-model="bookingForm.endTime" class="form-input" required>
                    <option v-for="time in endTimeOptions" :key="time" :value="time">{{ time }}</option>
                  </select>
                </div>
              </div>
              <div>
                <label class="form-label">Ghi chú</label>
                <textarea v-model="bookingForm.note" class="form-input" rows="2" placeholder="Ghi chú thêm..."></textarea>
              </div>
              <div class="flex justify-end gap-3 pt-4">
                <button type="button" @click="showBookingModal = false" class="btn-secondary">Hủy</button>
                <button type="submit" class="btn-primary" :disabled="submitting">
                  {{ submitting ? 'Đang đặt...' : 'Xác nhận đặt sân' }}
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
import { bookingsApi } from '@/api/bookings.api'
import { courtsApi } from '@/api/courts.api'
import MainLayout from '@/components/layout/MainLayout.vue'
import { formatCurrency, formatDate, getStatusLabel } from '@/utils/format'
import dayjs from 'dayjs'
import { computed, onMounted, reactive, ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()

const loading = ref(true)
const courts = ref([])
const bookings = ref([])
const myBookings = ref([])
const selectedDate = ref(dayjs().format('YYYY-MM-DD'))
const selectedCourtId = ref('')
const showBookingModal = ref(false)
const submitting = ref(false)

const today = dayjs().format('YYYY-MM-DD')

const bookingForm = reactive({
  courtId: '',
  date: today,
  startTime: '08:00',
  endTime: '09:00',
  note: ''
})

const timeSlots = [
  '06:00', '07:00', '08:00', '09:00', '10:00', '11:00',
  '12:00', '13:00', '14:00', '15:00', '16:00', '17:00',
  '18:00', '19:00', '20:00', '21:00'
]

const timeOptions = timeSlots.slice(0, -1)

const endTimeOptions = computed(() => {
  const startIdx = timeSlots.indexOf(bookingForm.startTime)
  return timeSlots.slice(startIdx + 1)
})

const displayCourts = computed(() => {
  if (selectedCourtId.value) {
    return courts.value.filter(c => c.id === parseInt(selectedCourtId.value))
  }
  return courts.value.slice(0, 4) // Limit to 4 courts for display
})

const getSlotClass = (courtId, time) => {
  const booking = bookings.value.find(b => 
    b.courtId === courtId && 
    b.startTime <= time && 
    b.endTime > time
  )
  
  if (!booking) return 'bg-green-50 border border-green-200 hover:bg-green-100'
  if (booking.status === 'Pending') return 'bg-yellow-50 border border-yellow-200'
  return 'bg-red-50 border border-red-200'
}

const getSlotText = (courtId, time) => {
  const booking = bookings.value.find(b => 
    b.courtId === courtId && 
    b.startTime <= time && 
    b.endTime > time
  )
  
  if (!booking) return 'Trống'
  return booking.memberName || 'Đã đặt'
}

const getStatusBadgeClass = (status) => {
  const classes = {
    'Pending': 'badge-warning',
    'Confirmed': 'badge-success',
    'Cancelled': 'badge-danger',
    'Completed': 'badge-info'
  }
  return classes[status] || 'badge-info'
}

const handleSlotClick = (court, time) => {
  const booking = bookings.value.find(b => 
    b.courtId === court.id && 
    b.startTime <= time && 
    b.endTime > time
  )
  
  if (!booking) {
    bookingForm.courtId = court.id
    bookingForm.date = selectedDate.value
    bookingForm.startTime = time
    const nextIdx = timeSlots.indexOf(time) + 1
    bookingForm.endTime = timeSlots[nextIdx] || '22:00'
    showBookingModal.value = true
  }
}

const openBookingModal = () => {
  bookingForm.courtId = ''
  bookingForm.date = today
  bookingForm.startTime = '08:00'
  bookingForm.endTime = '09:00'
  bookingForm.note = ''
  showBookingModal.value = true
}

const submitBooking = async () => {
  submitting.value = true
  try {
    await bookingsApi.create(bookingForm)
    toast.success('Đặt sân thành công! Chờ xác nhận từ admin.')
    showBookingModal.value = false
    fetchBookings()
    fetchMyBookings()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể đặt sân')
  } finally {
    submitting.value = false
  }
}

const cancelBooking = async (booking) => {
  try {
    await bookingsApi.cancel(booking.id)
    toast.success('Đã hủy đặt sân')
    fetchMyBookings()
    fetchBookings()
  } catch (error) {
    toast.error('Không thể hủy đặt sân')
  }
}

const fetchCourts = async () => {
  try {
    const response = await courtsApi.getAll()
    courts.value = response.data?.data || response.data || []
  } catch (error) {
    console.error('Error fetching courts:', error)
  }
}

const fetchBookings = async () => {
  loading.value = true
  try {
    const response = await bookingsApi.getByDate(selectedDate.value)
    bookings.value = response.data?.data || response.data || []
  } catch (error) {
    console.error('Error fetching bookings:', error)
  } finally {
    loading.value = false
  }
}

const fetchMyBookings = async () => {
  try {
    const response = await bookingsApi.getMyBookings()
    myBookings.value = response.data?.data || response.data || []
  } catch (error) {
    console.error('Error fetching my bookings:', error)
  }
}

onMounted(async () => {
  await fetchCourts()
  await fetchBookings()
  await fetchMyBookings()
})
</script>
