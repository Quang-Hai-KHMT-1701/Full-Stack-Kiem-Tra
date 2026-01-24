<template>
  <MainLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Quản lý thu chi</h1>
          <p class="page-subtitle">Theo dõi các khoản thu chi của câu lạc bộ</p>
        </div>
        <button v-if="authStore.hasRole('Admin') || authStore.hasRole('Treasurer')" @click="openCreateModal" class="btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm giao dịch
        </button>
      </div>
      
      <!-- Summary Cards -->
      <div class="grid md:grid-cols-3 gap-4">
        <div class="card p-6">
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 11l5-5m0 0l5 5m-5-5v12" />
              </svg>
            </div>
            <div>
              <p class="text-sm text-gray-500">Tổng thu</p>
              <p class="text-2xl font-bold text-green-600">{{ formatCurrency(summary.totalIncome) }}</p>
            </div>
          </div>
        </div>
        
        <div class="card p-6">
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 bg-red-100 rounded-xl flex items-center justify-center">
              <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 13l-5 5m0 0l-5-5m5 5V6" />
              </svg>
            </div>
            <div>
              <p class="text-sm text-gray-500">Tổng chi</p>
              <p class="text-2xl font-bold text-red-600">{{ formatCurrency(summary.totalExpense) }}</p>
            </div>
          </div>
        </div>
        
        <div class="card p-6">
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 bg-primary-100 rounded-xl flex items-center justify-center">
              <svg class="w-6 h-6 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
              </svg>
            </div>
            <div>
              <p class="text-sm text-gray-500">Số dư</p>
              <p :class="['text-2xl font-bold', summary.balance >= 0 ? 'text-primary-600' : 'text-red-600']">
                {{ formatCurrency(summary.balance) }}
              </p>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Filters -->
      <div class="card p-4">
        <div class="flex flex-wrap gap-4">
          <div>
            <label class="form-label">Loại</label>
            <select v-model="filters.type" class="form-input">
              <option value="">Tất cả</option>
              <option value="Income">Thu</option>
              <option value="Expense">Chi</option>
            </select>
          </div>
          <div>
            <label class="form-label">Danh mục</label>
            <select v-model="filters.categoryId" class="form-input">
              <option value="">Tất cả</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </option>
            </select>
          </div>
          <div>
            <label class="form-label">Từ ngày</label>
            <input type="date" v-model="filters.fromDate" class="form-input" />
          </div>
          <div>
            <label class="form-label">Đến ngày</label>
            <input type="date" v-model="filters.toDate" class="form-input" />
          </div>
        </div>
      </div>
      
      <!-- Transactions List -->
      <div class="card">
        <div class="card-header">
          <h3 class="font-semibold">Danh sách giao dịch</h3>
        </div>
        <div class="card-body p-0">
          <div v-if="loading" class="p-8">
            <div v-for="i in 5" :key="i" class="flex items-center justify-between py-4 border-b border-gray-100">
              <div class="flex items-center gap-4">
                <div class="skeleton w-10 h-10 rounded-full"></div>
                <div class="space-y-2">
                  <div class="skeleton h-4 w-32"></div>
                  <div class="skeleton h-3 w-24"></div>
                </div>
              </div>
              <div class="skeleton h-5 w-24"></div>
            </div>
          </div>
          
          <div v-else-if="filteredTransactions.length === 0" class="p-12 text-center text-gray-500">
            Chưa có giao dịch nào
          </div>
          
          <div v-else class="divide-y divide-gray-100">
            <div 
              v-for="transaction in filteredTransactions" 
              :key="transaction.id"
              class="flex items-center justify-between p-4 hover:bg-gray-50"
            >
              <div class="flex items-center gap-4">
                <div :class="[
                  'w-10 h-10 rounded-full flex items-center justify-center',
                  transaction.type === 'Income' ? 'bg-green-100' : 'bg-red-100'
                ]">
                  <svg 
                    class="w-5 h-5" 
                    :class="transaction.type === 'Income' ? 'text-green-600' : 'text-red-600'"
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path 
                      v-if="transaction.type === 'Income'"
                      stroke-linecap="round" 
                      stroke-linejoin="round" 
                      stroke-width="2" 
                      d="M7 11l5-5m0 0l5 5m-5-5v12" 
                    />
                    <path 
                      v-else
                      stroke-linecap="round" 
                      stroke-linejoin="round" 
                      stroke-width="2" 
                      d="M17 13l-5 5m0 0l-5-5m5 5V6" 
                    />
                  </svg>
                </div>
                <div>
                  <p class="font-medium text-gray-900">{{ transaction.description }}</p>
                  <p class="text-sm text-gray-500">
                    {{ transaction.category?.name || 'Không có danh mục' }} • {{ formatDate(transaction.transactionDate) }}
                  </p>
                </div>
              </div>
              <div class="flex items-center gap-4">
                <span :class="[
                  'font-bold',
                  transaction.type === 'Income' ? 'text-green-600' : 'text-red-600'
                ]">
                  {{ transaction.type === 'Income' ? '+' : '-' }}{{ formatCurrency(transaction.amount) }}
                </span>
                <div v-if="authStore.hasRole('Admin') || authStore.hasRole('Treasurer')" class="flex gap-2">
                  <button @click="editTransaction(transaction)" class="p-1 hover:bg-gray-100 rounded">
                    <svg class="w-5 h-5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                  <button @click="confirmDelete(transaction)" class="p-1 hover:bg-gray-100 rounded">
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
          <div class="bg-white rounded-xl shadow-xl max-w-md w-full">
            <div class="px-6 py-4 border-b border-gray-100">
              <h3 class="text-lg font-semibold">{{ isEditing ? 'Chỉnh sửa' : 'Thêm' }} giao dịch</h3>
            </div>
            <form @submit.prevent="submitTransaction" class="p-6 space-y-4">
              <div>
                <label class="form-label">Loại giao dịch *</label>
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
                <label class="form-label">Danh mục *</label>
                <select v-model="form.categoryId" class="form-input" required>
                  <option value="">Chọn danh mục</option>
                  <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                    {{ cat.name }}
                  </option>
                </select>
              </div>
              <div>
                <label class="form-label">Số tiền *</label>
                <input v-model.number="form.amount" type="number" class="form-input" required min="1" placeholder="VD: 100000" />
              </div>
              <div>
                <label class="form-label">Ngày *</label>
                <input v-model="form.transactionDate" type="date" class="form-input" required />
              </div>
              <div>
                <label class="form-label">Mô tả *</label>
                <textarea v-model="form.description" class="form-input" rows="2" required placeholder="Nội dung giao dịch"></textarea>
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
      message="Bạn có chắc chắn muốn xóa giao dịch này?"
      @confirm="deleteTransaction"
      @cancel="showDeleteConfirm = false"
    />
  </MainLayout>
</template>

<script setup>
import { transactionsApi } from '@/api/transactions.api'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'
import Pagination from '@/components/common/Pagination.vue'
import MainLayout from '@/components/layout/MainLayout.vue'
import { useAuthStore } from '@/stores/auth.store'
import { formatCurrency, formatDate } from '@/utils/format'
import dayjs from 'dayjs'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const transactions = ref([])
const categories = ref([])
const showModal = ref(false)
const showDeleteConfirm = ref(false)
const isEditing = ref(false)
const saving = ref(false)
const selectedTransaction = ref(null)

const summary = reactive({
  totalIncome: 0,
  totalExpense: 0,
  balance: 0
})

const filters = reactive({
  type: '',
  categoryId: '',
  fromDate: '',
  toDate: ''
})

const pagination = reactive({
  page: 1,
  pageSize: 10,
  totalPages: 1
})

const form = reactive({
  type: 'Income',
  categoryId: '',
  amount: '',
  transactionDate: dayjs().format('YYYY-MM-DD'),
  description: ''
})

const filteredTransactions = computed(() => {
  let result = [...transactions.value]
  
  if (filters.type) {
    result = result.filter(t => t.type === filters.type)
  }
  
  if (filters.categoryId) {
    result = result.filter(t => t.categoryId === parseInt(filters.categoryId))
  }
  
  if (filters.fromDate) {
    result = result.filter(t => t.transactionDate >= filters.fromDate)
  }
  
  if (filters.toDate) {
    result = result.filter(t => t.transactionDate <= filters.toDate)
  }
  
  return result
})

const fetchTransactions = async () => {
  loading.value = true
  try {
    const response = await transactionsApi.getAll({
      page: pagination.page,
      pageSize: pagination.pageSize
    })
    const data = response.data?.data || response.data || []
    transactions.value = data.items || data
    pagination.totalPages = data.totalPages || 1
    
    // Calculate summary
    const all = transactions.value
    summary.totalIncome = all.filter(t => t.type === 'Income').reduce((sum, t) => sum + t.amount, 0)
    summary.totalExpense = all.filter(t => t.type === 'Expense').reduce((sum, t) => sum + t.amount, 0)
    summary.balance = summary.totalIncome - summary.totalExpense
  } catch (error) {
    toast.error('Không thể tải danh sách giao dịch')
  } finally {
    loading.value = false
  }
}

const fetchCategories = async () => {
  try {
    const response = await transactionsApi.getCategories()
    categories.value = response.data?.data || response.data || []
  } catch (error) {
    console.error('Error fetching categories:', error)
  }
}

const openCreateModal = () => {
  isEditing.value = false
  Object.assign(form, {
    type: 'Income',
    categoryId: '',
    amount: '',
    transactionDate: dayjs().format('YYYY-MM-DD'),
    description: ''
  })
  showModal.value = true
}

const editTransaction = (transaction) => {
  isEditing.value = true
  selectedTransaction.value = transaction
  Object.assign(form, {
    type: transaction.type,
    categoryId: transaction.categoryId,
    amount: transaction.amount,
    transactionDate: dayjs(transaction.transactionDate).format('YYYY-MM-DD'),
    description: transaction.description
  })
  showModal.value = true
}

const submitTransaction = async () => {
  saving.value = true
  try {
    if (isEditing.value) {
      await transactionsApi.update(selectedTransaction.value.id, form)
      toast.success('Cập nhật thành công!')
    } else {
      await transactionsApi.create(form)
      toast.success('Thêm giao dịch thành công!')
    }
    showModal.value = false
    fetchTransactions()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể lưu giao dịch')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (transaction) => {
  selectedTransaction.value = transaction
  showDeleteConfirm.value = true
}

const deleteTransaction = async () => {
  try {
    await transactionsApi.delete(selectedTransaction.value.id)
    toast.success('Đã xóa giao dịch')
    showDeleteConfirm.value = false
    fetchTransactions()
  } catch (error) {
    toast.error('Không thể xóa')
  }
}

const changePage = (page) => {
  pagination.page = page
  fetchTransactions()
}

watch(filters, () => {
  pagination.page = 1
})

onMounted(() => {
  fetchCategories()
  fetchTransactions()
})
</script>
