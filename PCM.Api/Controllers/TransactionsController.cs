using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Treasurer")]  // Chỉ Admin và Thủ quỹ mới xem được
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/transactions
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _context.Transactions.AsQueryable();

            // Filter by type
            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type == type.ToLower());

            // Filter by category
            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            // Filter by date range
            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // =========================
        // GET: api/transactions/{id}
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { message = $"Giao dịch với id {id} không tồn tại" });

            return Ok(transaction);
        }

        // =========================
        // GET: api/transactions/summary
        // =========================
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _context.Transactions.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            var transactions = await query.ToListAsync();

            var income = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);
            var balance = income - expense;

            // 🚨 Cảnh báo Quỹ âm
            bool isNegativeBalance = balance < 0;
            string? warning = isNegativeBalance
                ? "⚠️ CẢNH BÁO: Quỹ CLB đang bị ÂM! Cần thu thêm hoặc giảm chi."
                : null;

            return Ok(new
            {
                income,
                expense,
                balance,
                totalTransactions = transactions.Count,
                period = new { fromDate, toDate },
                isNegativeBalance,
                warning
            });
        }

        // =========================
        // GET: api/transactions/by-category
        // =========================
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _context.Transactions.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            var grouped = await query
                .GroupBy(t => new { t.CategoryId, t.CategoryName, t.Type })
                .Select(g => new
                {
                    categoryId = g.Key.CategoryId,
                    categoryName = g.Key.CategoryName,
                    type = g.Key.Type,
                    totalAmount = g.Sum(t => t.Amount),
                    count = g.Count()
                })
                .OrderByDescending(x => x.totalAmount)
                .ToListAsync();

            return Ok(grouped);
        }

        // =========================
        // POST: api/transactions
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Lấy CategoryName từ database
            var category = await _context.TransactionCategories.FindAsync(dto.CategoryId);
            var categoryName = category?.Name ?? "Khác";
            var categoryType = category?.Type ?? dto.Type.ToLower();

            var transaction = new Transaction
            {
                Description = dto.Description,
                Amount = dto.Amount,
                Type = categoryType,
                CategoryId = dto.CategoryId,
                CategoryName = categoryName,
                TransactionDate = dto.TransactionDate ?? DateTime.Now,
                Notes = dto.Notes,
                MemberId = dto.MemberId,
                BookingId = dto.BookingId,
                ChallengeId = dto.ChallengeId,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        // =========================
        // PUT: api/transactions/{id}
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTransactionDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { message = $"Giao dịch với id {id} không tồn tại" });

            // Lấy CategoryName từ database
            var category = await _context.TransactionCategories.FindAsync(dto.CategoryId);
            var categoryName = category?.Name ?? "Khác";
            var categoryType = category?.Type ?? dto.Type.ToLower();

            transaction.Description = dto.Description;
            transaction.Amount = dto.Amount;
            transaction.Type = categoryType;
            transaction.CategoryId = dto.CategoryId;
            transaction.CategoryName = categoryName;
            transaction.TransactionDate = dto.TransactionDate ?? transaction.TransactionDate;
            transaction.Notes = dto.Notes;
            transaction.MemberId = dto.MemberId;
            transaction.BookingId = dto.BookingId;
            transaction.ChallengeId = dto.ChallengeId;

            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        // =========================
        // DELETE: api/transactions/{id}
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { message = $"Giao dịch với id {id} không tồn tại" });

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // GET: api/transactions/monthly-report
        // =========================
        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;

            var transactions = await _context.Transactions
                .Where(t => t.TransactionDate.Year == year)
                .ToListAsync();

            var report = Enumerable.Range(1, 12).Select(month => new
            {
                month,
                monthName = new DateTime(year, month, 1).ToString("MMMM"),
                income = transactions
                    .Where(t => t.TransactionDate.Month == month && t.Type == "income")
                    .Sum(t => t.Amount),
                expense = transactions
                    .Where(t => t.TransactionDate.Month == month && t.Type == "expense")
                    .Sum(t => t.Amount)
            }).ToList();

            return Ok(new { year, data = report });
        }
    }

    // DTO for creating/updating transactions
    public class CreateTransactionDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "income";
        public int CategoryId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? Notes { get; set; }
        public int? MemberId { get; set; }
        public int? BookingId { get; set; }
        public int? ChallengeId { get; set; }
        public string? CreatedBy { get; set; }
    }
}
