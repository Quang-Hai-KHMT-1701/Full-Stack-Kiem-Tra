using PCM.Api.Models.Core;
using PCM.Api.Models.Sports;

namespace PCM.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // DbSeeder chỉ seed thêm data demo nếu chưa có
            // SeedData đã seed Courts và Members cơ bản

            // Seed thêm News mẫu nếu chưa có
            if (!context.News.Any())
            {
                context.News.AddRange(
                    new News
                    {
                        Title = "Chào mừng đến với CLB Pickleball",
                        Content = "Chào mừng tất cả thành viên mới và cũ đến với CLB Pickleball của chúng ta! Hãy cùng nhau tận hưởng môn thể thao tuyệt vời này.",
                        Summary = "Chào mừng thành viên mới!",
                        IsPinned = true,
                        Status = "Published",
                        CreatedBy = "Admin",
                        CreatedDate = DateTime.Now.AddDays(-7)
                    },
                    new News
                    {
                        Title = "Lịch tập luyện tháng 1/2026",
                        Content = "Lịch tập luyện hàng tuần:\n- Thứ 3, 5: 18:00 - 21:00\n- Thứ 7: 14:00 - 18:00\n- Chủ nhật: 08:00 - 12:00",
                        Summary = "Cập nhật lịch tập mới",
                        IsPinned = false,
                        Status = "Published",
                        CreatedBy = "Admin",
                        CreatedDate = DateTime.Now.AddDays(-3)
                    },
                    new News
                    {
                        Title = "Giải đấu mùa xuân 2026",
                        Content = "CLB sẽ tổ chức giải đấu mùa xuân vào tháng 2/2026. Các thành viên quan tâm vui lòng đăng ký trước ngày 15/02.",
                        Summary = "Thông báo giải đấu sắp tới",
                        IsPinned = true,
                        Status = "Published",
                        CreatedBy = "Admin",
                        CreatedDate = DateTime.Now.AddDays(-1)
                    }
                );
            }

            // Seed thêm Transactions mẫu nếu chưa có
            if (!context.Transactions.Any())
            {
                context.Transactions.AddRange(
                    new Transaction
                    {
                        Description = "Thu phí thành viên tháng 1",
                        Amount = 500000,
                        Type = "income",
                        CategoryId = 1,
                        CategoryName = "Thu phí thành viên",
                        TransactionDate = DateTime.Now.AddDays(-10),
                        CreatedBy = "Admin"
                    },
                    new Transaction
                    {
                        Description = "Thu phí sân ngày 20/01",
                        Amount = 300000,
                        Type = "income",
                        CategoryId = 2,
                        CategoryName = "Thu phí sân",
                        TransactionDate = DateTime.Now.AddDays(-5),
                        CreatedBy = "Admin"
                    },
                    new Transaction
                    {
                        Description = "Chi phí điện nước tháng 1",
                        Amount = 200000,
                        Type = "expense",
                        CategoryId = 6,
                        CategoryName = "Chi phí vận hành",
                        TransactionDate = DateTime.Now.AddDays(-2),
                        CreatedBy = "Admin"
                    }
                );
            }

            context.SaveChanges();
        }
    }
}
