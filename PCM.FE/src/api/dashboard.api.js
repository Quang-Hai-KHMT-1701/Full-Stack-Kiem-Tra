import api from './axios'

export const dashboardApi = {
  // Lấy thống kê tổng quan (Admin/Treasurer)
  getStats: () => api.get('/dashboard/stats'),
  
  // Lấy top members theo rank
  getTopMembers: (limit = 5) => api.get('/dashboard/top-members', { params: { limit } }),
  
  // Lấy tin ghim
  getPinnedNews: () => api.get('/dashboard/pinned-news'),
  
  // Lấy tin mới nhất
  getRecentNews: (limit = 5) => api.get('/dashboard/recent-news', { params: { limit } }),
  
  // Lấy challenges sắp tới
  getUpcomingChallenges: (limit = 5) => api.get('/dashboard/upcoming-challenges', { params: { limit } }),
  
  // Lấy giao dịch gần đây (Admin/Treasurer)
  getRecentTransactions: (limit = 10) => api.get('/dashboard/recent-transactions', { params: { limit } }),
  
  // Lấy booking hôm nay
  getTodayBookings: () => api.get('/dashboard/today-bookings'),
  
  // Lấy tổng hợp tài chính theo tháng (Admin/Treasurer)
  getFinanceSummary: (months = 6) => api.get('/dashboard/finance-summary', { params: { months } }),
  
  // Lấy thống kê theo danh mục (Admin/Treasurer)
  getCategorySummary: () => api.get('/dashboard/category-summary'),
}

export default dashboardApi
