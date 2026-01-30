using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/dashboard/stats - Thống kê tổng quan
        // =========================
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetStats()
        {
            var totalMembers = await _context.Members.CountAsync();
            var activeMembers = await _context.Members.CountAsync(m => m.IsActive);
            var totalCourts = await _context.Courts.CountAsync();
            var activeCourts = await _context.Courts.CountAsync(c => c.IsActive);
            var totalChallenges = await _context.Challenges.CountAsync();
            var openChallenges = await _context.Challenges.CountAsync(c => c.Status == "Open");
            var ongoingChallenges = await _context.Challenges.CountAsync(c => c.Status == "Ongoing");
            var totalMatches = await _context.Matches.CountAsync();
            var todayBookings = await _context.Bookings.CountAsync(b => b.StartTime.Date == DateTime.Today);

            // Thống kê tài chính
            var totalIncome = await _context.Transactions
                .Where(t => t.Type == "Income")
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalExpense = await _context.Transactions
                .Where(t => t.Type == "Expense")
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var balance = totalIncome - totalExpense;

            // Thống kê theo tháng hiện tại
            var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var monthlyIncome = await _context.Transactions
                .Where(t => t.Type == "Income" && t.TransactionDate >= firstDayOfMonth)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var monthlyExpense = await _context.Transactions
                .Where(t => t.Type == "Expense" && t.TransactionDate >= firstDayOfMonth)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            return Ok(new
            {
                members = new
                {
                    total = totalMembers,
                    active = activeMembers
                },
                courts = new
                {
                    total = totalCourts,
                    active = activeCourts
                },
                challenges = new
                {
                    total = totalChallenges,
                    open = openChallenges,
                    ongoing = ongoingChallenges
                },
                matches = new
                {
                    total = totalMatches
                },
                bookings = new
                {
                    today = todayBookings
                },
                finance = new
                {
                    balance,
                    totalIncome,
                    totalExpense,
                    monthlyIncome,
                    monthlyExpense,
                    isNegative = balance < 0
                }
            });
        }

        // =========================
        // GET: api/dashboard/top-members - BXH Top 5 thành viên theo Rank
        // =========================
        [HttpGet("top-members")]
        public async Task<IActionResult> GetTopMembers([FromQuery] int limit = 5)
        {
            var topMembers = await _context.Members
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.RankLevel)
                .Take(limit)
                .Select(m => new
                {
                    m.Id,
                    m.FullName,
                    m.RankLevel,
                    m.PhoneNumber,
                    m.Email
                })
                .ToListAsync();

            return Ok(topMembers);
        }

        // =========================
        // GET: api/dashboard/pinned-news - Tin ghim
        // =========================
        [HttpGet("pinned-news")]
        public async Task<IActionResult> GetPinnedNews()
        {
            var pinnedNews = await _context.News
                .Where(n => n.IsPinned && n.Status == "Published")
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Summary,
                    n.ImageUrl,
                    PublishedDate = n.CreatedDate
                })
                .ToListAsync();

            return Ok(pinnedNews);
        }

        // =========================
        // GET: api/dashboard/recent-news - Tin mới nhất
        // =========================
        [HttpGet("recent-news")]
        public async Task<IActionResult> GetRecentNews([FromQuery] int limit = 5)
        {
            var recentNews = await _context.News
                .Where(n => n.Status == "Published")
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Summary,
                    n.ImageUrl,
                    PublishedDate = n.CreatedDate,
                    n.IsPinned
                })
                .ToListAsync();

            return Ok(recentNews);
        }

        // =========================
        // GET: api/dashboard/upcoming-challenges - Các giải đấu sắp tới
        // =========================
        [HttpGet("upcoming-challenges")]
        public async Task<IActionResult> GetUpcomingChallenges([FromQuery] int limit = 5)
        {
            var upcomingChallenges = await _context.Challenges
                .Where(c => c.Status == "Open" || c.Status == "Ongoing")
                .OrderBy(c => c.StartDate)
                .Take(limit)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Type,
                    c.GameMode,
                    c.Status,
                    c.StartDate,
                    c.EndDate,
                    c.MaxParticipants,
                    ParticipantCount = c.Participants.Count,
                    c.EntryFee,
                    c.PrizePool
                })
                .ToListAsync();

            return Ok(upcomingChallenges);
        }

        // =========================
        // GET: api/dashboard/recent-transactions - Giao dịch gần đây
        // =========================
        [HttpGet("recent-transactions")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 10)
        {
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(limit)
                .ToListAsync();

            var result = new List<object>();
            foreach (var t in transactions)
            {
                var categoryName = t.CategoryName ?? "Không xác định";
                if (t.CategoryId > 0 && string.IsNullOrEmpty(t.CategoryName))
                {
                    var cat = await _context.TransactionCategories.FindAsync(t.CategoryId);
                    categoryName = cat?.Name ?? "Không xác định";
                }

                string? memberName = null;
                if (t.MemberId.HasValue)
                {
                    var member = await _context.Members.FindAsync(t.MemberId.Value);
                    memberName = member?.FullName;
                }

                result.Add(new
                {
                    t.Id,
                    t.Amount,
                    t.Type,
                    CategoryName = categoryName,
                    MemberName = memberName ?? "N/A",
                    t.Description,
                    t.TransactionDate
                });
            }

            return Ok(result);
        }

        // =========================
        // GET: api/dashboard/today-bookings - Booking hôm nay
        // =========================
        [HttpGet("today-bookings")]
        public async Task<IActionResult> GetTodayBookings()
        {
            var todayBookings = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .Where(b => b.StartTime.Date == DateTime.Today && b.Status != Enums.BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .Select(b => new
                {
                    b.Id,
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.MemberId,
                    MemberName = b.Member.FullName,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            return Ok(todayBookings);
        }

        // =========================
        // GET: api/dashboard/finance-summary - Tổng hợp tài chính theo tháng
        // =========================
        [HttpGet("finance-summary")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetFinanceSummary([FromQuery] int months = 6)
        {
            var summaries = new List<object>();

            for (int i = 0; i < months; i++)
            {
                var date = DateTime.Today.AddMonths(-i);
                var firstDay = new DateTime(date.Year, date.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                var income = await _context.Transactions
                    .Where(t => t.Type == "Income" && t.TransactionDate >= firstDay && t.TransactionDate <= lastDay)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                var expense = await _context.Transactions
                    .Where(t => t.Type == "Expense" && t.TransactionDate >= firstDay && t.TransactionDate <= lastDay)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                summaries.Add(new
                {
                    month = firstDay.ToString("yyyy-MM"),
                    monthName = firstDay.ToString("MM/yyyy"),
                    income,
                    expense,
                    balance = income - expense
                });
            }

            summaries.Reverse();
            return Ok(summaries);
        }

        // =========================
        // GET: api/dashboard/category-summary - Thống kê theo danh mục
        // =========================
        [HttpGet("category-summary")]
        [Authorize(Roles = "Admin,Treasurer")]
        public async Task<IActionResult> GetCategorySummary()
        {
            var categories = await _context.TransactionCategories.ToListAsync();

            var summary = new List<object>();
            foreach (var cat in categories)
            {
                var total = await _context.Transactions
                    .Where(t => t.CategoryId == cat.Id)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                var count = await _context.Transactions
                    .Where(t => t.CategoryId == cat.Id)
                    .CountAsync();

                summary.Add(new
                {
                    categoryId = cat.Id,
                    categoryName = cat.Name,
                    type = cat.Type,
                    totalAmount = total,
                    transactionCount = count
                });
            }

            return Ok(summary);
        }
    }
}
