using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/transaction-categories")]
    public class TransactionCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/transaction-categories
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type = null)
        {
            var query = _context.TransactionCategories.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type == type.ToLower());
            }

            var categories = await query
                .Where(c => c.IsActive)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        // =========================
        // GET: api/transaction-categories/{id}
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.TransactionCategories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = $"Danh mục với id {id} không tồn tại" });

            return Ok(category);
        }

        // =========================
        // GET: api/transaction-categories/by-type/{type}
        // =========================
        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            var categories = await _context.TransactionCategories
                .Where(c => c.Type == type.ToLower() && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        // =========================
        // POST: api/transaction-categories
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra tên đã tồn tại
            var exists = await _context.TransactionCategories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Type == dto.Type.ToLower());

            if (exists)
                return BadRequest(new { message = $"Danh mục '{dto.Name}' loại '{dto.Type}' đã tồn tại" });

            var category = new TransactionCategory
            {
                Name = dto.Name,
                Type = dto.Type.ToLower(),
                Description = dto.Description,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.TransactionCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // =========================
        // PUT: api/transaction-categories/{id}
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
        {
            var category = await _context.TransactionCategories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = $"Danh mục với id {id} không tồn tại" });

            // Kiểm tra tên trùng với danh mục khác
            var exists = await _context.TransactionCategories
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower() && c.Type == dto.Type.ToLower());

            if (exists)
                return BadRequest(new { message = $"Danh mục '{dto.Name}' loại '{dto.Type}' đã tồn tại" });

            category.Name = dto.Name;
            category.Type = dto.Type.ToLower();
            category.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // =========================
        // DELETE: api/transaction-categories/{id}
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.TransactionCategories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = $"Danh mục với id {id} không tồn tại" });

            // Kiểm tra có transaction nào đang dùng không
            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.CategoryId == id);

            if (hasTransactions)
            {
                // Soft delete - chỉ đánh dấu inactive
                category.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Danh mục đã được vô hiệu hóa (có giao dịch liên quan)" });
            }

            // Hard delete
            _context.TransactionCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "income";
        public string? Description { get; set; }
    }
}
