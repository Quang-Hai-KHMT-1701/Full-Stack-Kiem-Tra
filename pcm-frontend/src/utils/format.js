/**
 * Format utilities
 */
import dayjs from 'dayjs'
import 'dayjs/locale/vi'

dayjs.locale('vi')

export const formatDate = (date, format = 'DD/MM/YYYY') => {
  if (!date) return ''
  return dayjs(date).format(format)
}

export const formatDateTime = (date) => {
  if (!date) return ''
  return dayjs(date).format('DD/MM/YYYY HH:mm')
}

export const formatTime = (time) => {
  if (!time) return ''
  return dayjs(time, 'HH:mm:ss').format('HH:mm')
}

export const formatCurrency = (amount) => {
  if (amount === null || amount === undefined) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

export const formatNumber = (num) => {
  if (num === null || num === undefined) return '0'
  return new Intl.NumberFormat('vi-VN').format(num)
}

export const truncateText = (text, length = 100) => {
  if (!text) return ''
  if (text.length <= length) return text
  return text.substring(0, length) + '...'
}

export const getStatusClass = (status) => {
  const classes = {
    'Open': 'badge-success',
    'Ongoing': 'badge-warning',
    'Finished': 'badge-info',
    'Cancelled': 'badge-danger',
    'Pending': 'badge-warning',
    'Confirmed': 'badge-success',
    'Rejected': 'badge-danger'
  }
  return classes[status] || 'badge-info'
}

export const getStatusLabel = (status) => {
  const labels = {
    'Open': 'Đang mở',
    'Ongoing': 'Đang diễn ra',
    'Finished': 'Đã kết thúc',
    'Cancelled': 'Đã hủy',
    'Pending': 'Chờ xác nhận',
    'Confirmed': 'Đã xác nhận',
    'Rejected': 'Đã từ chối'
  }
  return labels[status] || status
}
