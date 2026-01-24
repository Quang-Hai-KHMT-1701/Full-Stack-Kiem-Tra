# 🏓 Pickleball Club Management (PCM)

Hệ thống quản lý Câu lạc bộ Pickleball - Full Stack Application

## 📋 Mô tả dự án

PCM là một ứng dụng web full-stack để quản lý hoạt động của câu lạc bộ Pickleball, bao gồm:
- Quản lý thành viên
- Đặt sân
- Tổ chức giải đấu/thử thách
- Quản lý trận đấu và xếp hạng

## 🛠️ Công nghệ sử dụng

### Backend (PCM.Api)
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server + Entity Framework Core
- **Authentication**: JWT Bearer Token
- **Identity**: ASP.NET Core Identity
- **API Documentation**: Swagger/OpenAPI

### Frontend (pcm-frontend)
- **Framework**: Vue 3 (Composition API)
- **Build Tool**: Vite 5
- **State Management**: Pinia
- **Routing**: Vue Router 4
- **HTTP Client**: Axios
- **CSS Framework**: Tailwind CSS 3
- **Icons**: Heroicons

## 📁 Cấu trúc dự án

```
Tuan_7/
├── PCM.Api/                    # Backend .NET API
│   ├── Controllers/            # API Controllers
│   ├── Data/                   # DbContext & Seed data
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Enums/                  # Enumerations
│   ├── Migrations/             # EF Core Migrations
│   ├── Models/                 # Entity Models
│   │   ├── Core/              # Member, Court, Booking
│   │   ├── Identity/          # ApplicationUser
│   │   └── Sports/            # Challenge, Match, Participant
│   └── Program.cs             # Application entry point
│
└── pcm-frontend/              # Frontend Vue 3
    ├── src/
    │   ├── api/               # API modules (axios)
    │   ├── assets/css/        # Tailwind CSS styles
    │   ├── components/        # Vue components
    │   │   ├── common/        # Reusable components
    │   │   ├── dashboard/     # Dashboard widgets
    │   │   └── layout/        # Layout components
    │   ├── router/            # Vue Router config
    │   ├── stores/            # Pinia stores
    │   ├── utils/             # Helper functions
    │   └── views/             # Page components
    └── index.html
```

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống
- .NET 8.0 SDK
- Node.js 18+ 
- SQL Server (LocalDB hoặc full)

### 1. Clone repository
```bash
git clone https://github.com/Quang-Hai-KHMT-1701/Full-Stack-Kiem-Tra.git
cd Full-Stack-Kiem-Tra
```

### 2. Cài đặt Backend

```bash
cd PCM.Api

# Restore packages
dotnet restore

# Cập nhật connection string trong appsettings.json nếu cần

# Chạy migrations
dotnet ef database update

# Chạy API
dotnet run
```

Backend sẽ chạy tại: `http://localhost:5211`

### 3. Cài đặt Frontend

```bash
cd pcm-frontend

# Cài đặt dependencies
npm install

# Chạy development server
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:5173`

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/register` | Đăng ký tài khoản |
| POST | `/api/auth/login` | Đăng nhập |
| GET | `/api/auth/me` | Lấy thông tin user hiện tại |

### Members
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/members` | Danh sách thành viên |
| GET | `/api/members/{id}` | Chi tiết thành viên |
| POST | `/api/members` | Thêm thành viên |
| PUT | `/api/members/{id}` | Cập nhật thành viên |
| DELETE | `/api/members/{id}` | Xóa thành viên |
| GET | `/api/members/top-ranking` | Top xếp hạng |
| GET | `/api/members/stats` | Thống kê thành viên |

### Courts
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/courts` | Danh sách sân |
| GET | `/api/courts/active` | Sân đang hoạt động |
| POST | `/api/courts` | Thêm sân |
| PUT | `/api/courts/{id}` | Cập nhật sân |
| DELETE | `/api/courts/{id}` | Xóa sân |

### Bookings
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/bookings` | Danh sách đặt sân |
| POST | `/api/bookings` | Đặt sân mới |
| POST | `/api/bookings/{id}/confirm` | Xác nhận đặt sân |
| POST | `/api/bookings/{id}/cancel` | Hủy đặt sân |
| GET | `/api/bookings/available-slots` | Slot trống |
| GET | `/api/bookings/by-date` | Đặt sân theo ngày |

### Challenges
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/challenges` | Danh sách thử thách |
| POST | `/api/challenges` | Tạo thử thách |
| POST | `/api/challenges/{id}/join` | Tham gia |
| POST | `/api/challenges/{id}/leave` | Rời khỏi |
| POST | `/api/challenges/{id}/start` | Bắt đầu |
| POST | `/api/challenges/{id}/finish` | Kết thúc |

### Matches
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/matches` | Danh sách trận đấu |
| POST | `/api/matches` | Tạo trận đấu |
| GET | `/api/matches/recent` | Trận đấu gần đây |
| GET | `/api/matches/member/{id}` | Trận đấu của thành viên |

## 🔐 Tài khoản test

| Email | Password | Role |
|-------|----------|------|
| admin@pcm.com | Admin@123 | Admin |

## 📸 Screenshots

### Trang đăng nhập
- Giao diện đăng nhập với form email/password
- Xác thực JWT token

### Dashboard
- Thống kê tổng quan: số thành viên, sân, trận đấu
- Top xếp hạng thành viên
- Trận đấu gần đây

### Quản lý thành viên
- Danh sách thành viên với tìm kiếm, phân trang
- Thêm/sửa/xóa thành viên
- Xem chi tiết và lịch sử trận đấu

### Đặt sân
- Lịch đặt sân theo ngày
- Chọn khung giờ trống
- Xác nhận/hủy đặt sân

## 👨‍💻 Tác giả

- **Họ tên**: [Tên sinh viên]
- **MSSV**: [Mã số sinh viên]
- **Lớp**: [Lớp]

## 📄 License

MIT License - Xem file [LICENSE](LICENSE) để biết thêm chi tiết.
