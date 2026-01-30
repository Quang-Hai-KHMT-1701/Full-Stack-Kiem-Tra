using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/news
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? isPinned = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.News.AsQueryable();

            // Filter by pinned status
            if (isPinned.HasValue)
                query = query.Where(n => n.IsPinned == isPinned.Value);

            // Filter by status
            if (!string.IsNullOrEmpty(status))
                query = query.Where(n => n.Status == status);
            else
                query = query.Where(n => n.Status == "Published"); // Default only show published

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedDate)
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
        // GET: api/news/pinned
        // =========================
        [HttpGet("pinned")]
        public async Task<IActionResult> GetPinned()
        {
            var pinnedNews = await _context.News
                .Where(n => n.IsPinned && n.Status == "Published")
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToListAsync();

            return Ok(pinnedNews);
        }

        // =========================
        // GET: api/news/{id}
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound(new { message = $"Tin tức với id {id} không tồn tại" });

            return Ok(news);
        }

        // =========================
        // POST: api/news
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateNewsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var news = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                Summary = dto.Summary ?? (dto.Content.Length > 200 ? dto.Content.Substring(0, 200) + "..." : dto.Content),
                ImageUrl = dto.ImageUrl,
                IsPinned = dto.IsPinned,
                Status = dto.Status ?? "Published",
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return Ok(news);
        }

        // =========================
        // PUT: api/news/{id}
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateNewsDto dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound(new { message = $"Tin tức với id {id} không tồn tại" });

            news.Title = dto.Title;
            news.Content = dto.Content;
            news.Summary = dto.Summary ?? (dto.Content.Length > 200 ? dto.Content.Substring(0, 200) + "..." : dto.Content);
            news.ImageUrl = dto.ImageUrl;
            news.IsPinned = dto.IsPinned;
            news.Status = dto.Status ?? news.Status;
            news.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(news);
        }

        // =========================
        // DELETE: api/news/{id}
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound(new { message = $"Tin tức với id {id} không tồn tại" });

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // PATCH: api/news/{id}/pin
        // =========================
        [HttpPatch("{id}/pin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePin(int id, [FromBody] TogglePinDto dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound(new { message = $"Tin tức với id {id} không tồn tại" });

            news.IsPinned = dto.IsPinned;
            news.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(news);
        }

        // =========================
        // PATCH: api/news/{id}/status
        // =========================
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound(new { message = $"Tin tức với id {id} không tồn tại" });

            if (!new[] { "Draft", "Published", "Archived" }.Contains(dto.Status))
                return BadRequest(new { message = "Status phải là Draft, Published hoặc Archived" });

            news.Status = dto.Status;
            news.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(news);
        }
    }

    // DTOs
    public class CreateNewsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPinned { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class TogglePinDto
    {
        public bool IsPinned { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = "Published";
    }
}
