<template>
  <Teleport to="body">
    <Transition name="fade">
      <div 
        v-if="modelValue" 
        class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
        @click.self="cancel"
      >
        <div class="bg-white rounded-xl shadow-xl max-w-md w-full">
          <!-- Header -->
          <div class="px-6 py-4 border-b border-gray-100">
            <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
          </div>
          
          <!-- Body -->
          <div class="px-6 py-4">
            <p class="text-gray-600">{{ message }}</p>
          </div>
          
          <!-- Footer -->
          <div class="px-6 py-4 border-t border-gray-100 flex justify-end gap-3">
            <button 
              @click="cancel" 
              class="btn-secondary"
              :disabled="loading"
            >
              {{ cancelText }}
            </button>
            <button 
              @click="confirm" 
              :class="confirmButtonClass"
              :disabled="loading"
            >
              <LoadingSpinner v-if="loading" size="sm" class="mr-2" />
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import LoadingSpinner from './LoadingSpinner.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Xác nhận'
  },
  message: {
    type: String,
    default: 'Bạn có chắc chắn muốn thực hiện hành động này?'
  },
  confirmText: {
    type: String,
    default: 'Xác nhận'
  },
  cancelText: {
    type: String,
    default: 'Hủy'
  },
  type: {
    type: String,
    default: 'danger',
    validator: (value) => ['danger', 'warning', 'info', 'success'].includes(value)
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const confirmButtonClass = computed(() => {
  const classes = {
    danger: 'btn-danger',
    warning: 'btn-warning',
    info: 'btn-primary',
    success: 'btn-success'
  }
  return classes[props.type]
})

const cancel = () => {
  if (!props.loading) {
    emit('update:modelValue', false)
    emit('cancel')
  }
}

const confirm = () => {
  emit('confirm')
}
</script>
