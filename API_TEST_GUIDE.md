# API Test Guide - PCM (Pickleball Club Management)

## 🚀 Cách chạy trên VPS

### 1. SSH vào VPS
```bash
ssh root@143.198.88.205
cd /opt/app
```

### 2. Pull code mới và rebuild
```bash
git pull origin main
docker compose -f docker-compose.production.yml up -d --build api
```

### 3. Xem logs nếu có lỗi
```bash
docker compose -f docker-compose.production.yml logs -f api
```

---

## 📋 Danh sách API Endpoints

### Base URL
- Production: `https://api.tiemcamdo.linkpc.net`
- Development: `http://localhost:5241`

---

## 🔐 1. Authentication (Xác thực)

### 1.1 Đăng ký
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123",
  "fullName": "Nguyễn Văn Test",
  "phoneNumber": "0901234567"
}
```

**Response:**
```json
{
  "message": "Đăng ký thành công",
  "token": "eyJhbG...",
  "userId": "abc-123",
  "memberId": 1,
  "email": "test@example.com",
  "fullName": "Nguyễn Văn Test",
  "roles": ["Member"]
}
```

### 1.2 Đăng nhập
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@pcm.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "token": "eyJhbG...",
  "userId": "abc-123",
  "memberId": 1,
  "email": "admin@pcm.com",
  "fullName": "Quản trị viên",
  "phoneNumber": "0901234567",
  "roles": ["Admin"],
  "rankLevel": 5.0,
  "totalMatches": 0,
  "winMatches": 0
}
```

### 1.3 Lấy thông tin user hiện tại
```http
GET /api/auth/me
Authorization: Bearer {token}
```

---

## 👥 2. Members (Thành viên)

### 2.1 Danh sách thành viên
```http
GET /api/members
```

### 2.2 Chi tiết thành viên
```http
GET /api/members/{id}
```

### 2.3 Tạo thành viên (Admin)
```http
POST /api/members
Content-Type: application/json

{
  "fullName": "Nguyễn Văn A",
  "email": "a@example.com",
  "phoneNumber": "0912345678",
  "userId": "user-id-from-identity",
  "isActive": true,
  "rankLevel": 2.0
}
```

### 2.4 Top bảng xếp hạng
```http
GET /api/members/top-ranking?limit=5
```

### 2.5 Thống kê tổng quan
```http
GET /api/members/stats
```

---

## 🏸 3. Courts (Sân)

### 3.1 Danh sách sân
```http
GET /api/courts
```

### 3.2 Sân đang hoạt động
```http
GET /api/courts/active
```

### 3.3 Tạo sân mới
```http
POST /api/courts
Content-Type: application/json

{
  "name": "Sân E - VIP",
  "isActive": true
}
```

---

## 📅 4. Bookings (Đặt sân)

### 4.1 Danh sách booking
```http
GET /api/bookings
```

### 4.2 Đặt sân
```http
POST /api/bookings
Content-Type: application/json

{
  "courtId": 1,
  "memberId": 1,
  "startTime": "2026-01-30T09:00:00",
  "endTime": "2026-01-30T10:00:00",
  "notes": "Tập luyện cá nhân"
}
```

### 4.3 Xác nhận/Từ chối/Hủy booking
```http
POST /api/bookings/{id}/confirm
POST /api/bookings/{id}/reject
POST /api/bookings/{id}/cancel
```

### 4.4 Lấy slots có sẵn
```http
GET /api/bookings/available-slots?date=2026-01-30&courtId=1
```

### 4.5 Bookings theo ngày
```http
GET /api/bookings/by-date?date=2026-01-30
```

---

## 🏆 5. Challenges (Giải đấu)

### 5.1 Danh sách giải
```http
GET /api/challenges
```

### 5.2 Tạo giải mới (cần đăng nhập)
```http
POST /api/challenges
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Giải giao hữu tháng 2",
  "entryFee": 50000,
  "prizePool": 500000,
  "maxParticipants": 16,
  "startDate": "2026-02-15T08:00:00"
}
```

### 5.3 Tham gia giải
```http
POST /api/challenges/{id}/join
Authorization: Bearer {token}
```

### 5.4 Rời giải
```http
POST /api/challenges/{id}/leave
Authorization: Bearer {token}
```

### 5.5 Chia đội tự động
```http
POST /api/challenges/{id}/auto-divide-teams
Authorization: Bearer {token}
```

### 5.6 Đổi trạng thái giải
```http
POST /api/challenges/{id}/start
POST /api/challenges/{id}/finish
POST /api/challenges/{id}/cancel
```

---

## ⚔️ 6. Matches (Trận đấu)

### 6.1 Danh sách trận
```http
GET /api/matches
```

### 6.2 Tạo trận đấu
```http
POST /api/matches
Content-Type: application/json

{
  "challengeId": 1,
  "isRanked": true,
  "matchFormat": "3",
  "matchDate": "2026-01-30T10:00:00",
  "team1_Player1Id": 1,
  "team1_Player2Id": 2,
  "team2_Player1Id": 3,
  "team2_Player2Id": 4,
  "winningSide": "A"
}
```

### 6.3 Trận theo member
```http
GET /api/matches/member/{memberId}
```

### 6.4 Trận theo giải
```http
GET /api/matches/challenge/{challengeId}
```

---

## 💰 7. Transactions (Thu chi tài chính)

### 7.1 Danh sách giao dịch (có phân trang & filter)
```http
GET /api/transactions?page=1&pageSize=10&type=income&fromDate=2026-01-01
```

### 7.2 Tạo giao dịch
```http
POST /api/transactions
Content-Type: application/json

{
  "description": "Thu phí thành viên tháng 2",
  "amount": 500000,
  "type": "income",
  "categoryId": 1,
  "transactionDate": "2026-02-01",
  "memberId": 1,
  "createdBy": "Admin"
}
```

### 7.3 Tổng kết thu chi
```http
GET /api/transactions/summary?fromDate=2026-01-01&toDate=2026-01-31
```

### 7.4 Thống kê theo danh mục
```http
GET /api/transactions/by-category
```

### 7.5 Báo cáo hàng tháng
```http
GET /api/transactions/monthly-report?year=2026
```

---

## 📰 8. News (Tin tức)

### 8.1 Danh sách tin
```http
GET /api/news?page=1&pageSize=10&isPinned=true
```

### 8.2 Tin ghim
```http
GET /api/news/pinned
```

### 8.3 Tạo tin
```http
POST /api/news
Content-Type: application/json

{
  "title": "Thông báo mới",
  "content": "Nội dung chi tiết của tin tức...",
  "summary": "Tóm tắt ngắn gọn",
  "isPinned": false,
  "status": "Published",
  "createdBy": "Admin"
}
```

### 8.4 Ghim/bỏ ghim tin
```http
PATCH /api/news/{id}/pin
Content-Type: application/json

{
  "isPinned": true
}
```

### 8.5 Đổi trạng thái tin
```http
PATCH /api/news/{id}/status
Content-Type: application/json

{
  "status": "Draft" // hoặc "Published", "Archived"
}
```

---

## 📊 9. Categories (Danh mục tài chính)

### 9.1 Danh sách categories
```http
GET /api/categories
```

**Response:**
```json
[
  { "id": 1, "name": "Thu phí thành viên", "type": "income" },
  { "id": 2, "name": "Thu phí sân", "type": "income" },
  { "id": 3, "name": "Thu phí giải đấu", "type": "income" },
  { "id": 4, "name": "Tài trợ", "type": "income" },
  { "id": 5, "name": "Chi phí bảo trì", "type": "expense" },
  { "id": 6, "name": "Chi phí vận hành", "type": "expense" },
  { "id": 7, "name": "Chi phí giải đấu", "type": "expense" },
  { "id": 8, "name": "Chi phí khác", "type": "expense" }
]
```

### 9.2 Lọc theo loại
```http
GET /api/categories/by-type/income
GET /api/categories/by-type/expense
```

---

## 🏥 10. Health Check

```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-30T10:00:00Z",
  "database": "connected"
}
```

---

## 👤 Tài khoản test

| Email | Password | Role |
|-------|----------|------|
| admin@pcm.com | Admin@123 | Admin |
| member@pcm.com | Member@123 | Member |
| player1@pcm.com | Player@123 | Member |
| player2@pcm.com | Player@123 | Member |
| player3@pcm.com | Player@123 | Member |
| referee@pcm.com | Referee@123 | Referee |
| treasurer@pcm.com | Treasurer@123 | Treasurer |

---

## 🧪 Test với curl

### Đăng nhập và lấy token
```bash
# Đăng nhập
TOKEN=$(curl -s -X POST https://api.tiemcamdo.linkpc.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@pcm.com","password":"Admin@123"}' | jq -r '.token')

echo "Token: $TOKEN"

# Test endpoint có auth
curl -X GET https://api.tiemcamdo.linkpc.net/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

### Test các endpoint khác
```bash
# Health check
curl https://api.tiemcamdo.linkpc.net/health

# Members
curl https://api.tiemcamdo.linkpc.net/api/members

# Courts
curl https://api.tiemcamdo.linkpc.net/api/courts

# Categories
curl https://api.tiemcamdo.linkpc.net/api/categories

# Challenges
curl https://api.tiemcamdo.linkpc.net/api/challenges

# News
curl https://api.tiemcamdo.linkpc.net/api/news

# Transactions
curl https://api.tiemcamdo.linkpc.net/api/transactions
```

---

## ⚠️ Lưu ý

1. Các endpoint có `[Authorize]` cần header `Authorization: Bearer {token}`
2. Token có thời hạn 24 giờ (1440 phút)
3. Roles: Admin, Member, Referee, Treasurer
4. Categories là danh sách cố định, không thể thêm/sửa/xóa
5. Khi tạo Match, thống kê TotalMatches/WinMatches của Member sẽ tự động cập nhật
