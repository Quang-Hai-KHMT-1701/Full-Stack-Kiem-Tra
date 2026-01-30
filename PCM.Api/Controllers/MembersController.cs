using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/members - Ai cũng xem được danh sách
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] double? rankMin = null,
            [FromQuery] double? rankMax = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.Members.AsQueryable();

            // Search by name, email, phone
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m =>
                    m.FullName.Contains(search) ||
                    m.Email.Contains(search) ||
                    m.PhoneNumber.Contains(search));
            }

            // Filter by rank range
            if (rankMin.HasValue)
                query = query.Where(m => m.RankLevel >= rankMin.Value);

            if (rankMax.HasValue)
                query = query.Where(m => m.RankLevel <= rankMax.Value);

            // Filter by active status
            if (isActive.HasValue)
                query = query.Where(m => m.IsActive == isActive.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(m => m.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.FullName,
                    m.Email,
                    m.PhoneNumber,
                    m.JoinDate,
                    m.RankLevel,
                    m.IsActive,
                    m.TotalMatches,
                    m.WinMatches,
                    WinRate = m.TotalMatches > 0 ? Math.Round((double)m.WinMatches / m.TotalMatches * 100, 1) : 0,
                    m.UserId,
                    m.CreatedDate
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // GET: api/members/{id} - Ai cũng xem được chi tiết
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound(new { message = $"Thành viên với id {id} không tồn tại" });

            return Ok(new
            {
                member.Id,
                member.FullName,
                member.Email,
                member.PhoneNumber,
                member.JoinDate,
                member.RankLevel,
                member.IsActive,
                member.TotalMatches,
                member.WinMatches,
                WinRate = member.TotalMatches > 0 ? Math.Round((double)member.WinMatches / member.TotalMatches * 100, 1) : 0,
                member.UserId,
                member.CreatedDate
            });
        }

        // =========================
        // POST: api/members - Chỉ Admin mới tạo member
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check duplicate email
            if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
                return BadRequest(new { message = "Email đã tồn tại" });

            var member = new Member
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber ?? "",
                JoinDate = DateTime.Now,
                RankLevel = dto.RankLevel ?? 2.5,
                IsActive = true,
                TotalMatches = 0,
                WinMatches = 0,
                UserId = dto.UserId,
                CreatedDate = DateTime.Now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        // =========================
        // PUT: api/members/{id} - Admin sửa tất cả, Member chỉ sửa của mình
        // =========================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { message = $"Thành viên với id {id} không tồn tại" });

            // Kiểm tra quyền: Admin hoặc chính mình
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var isAdmin = User.IsInRole("Admin");
            var isOwner = member.UserId == userId;

            if (!isAdmin && !isOwner)
                return Forbid();

            // Cập nhật thông tin
            if (!string.IsNullOrEmpty(dto.FullName))
                member.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                member.PhoneNumber = dto.PhoneNumber;

            // Chỉ Admin mới được sửa các trường này
            if (isAdmin)
            {
                if (dto.RankLevel.HasValue)
                    member.RankLevel = dto.RankLevel.Value;

                if (dto.IsActive.HasValue)
                    member.IsActive = dto.IsActive.Value;

                if (!string.IsNullOrEmpty(dto.Email))
                    member.Email = dto.Email;
            }

            await _context.SaveChangesAsync();
            return Ok(member);
        }

        // =========================
        // DELETE: api/members/{id} - Chỉ Admin
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { message = $"Thành viên với id {id} không tồn tại" });

            // Soft delete - đánh dấu inactive thay vì xóa thật
            member.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã vô hiệu hóa thành viên" });
        }

        // =========================
        // GET: api/members/top-ranking
        // =========================
        [HttpGet("top-ranking")]
        public async Task<IActionResult> GetTopRanking([FromQuery] int limit = 5)
        {
            var topMembers = await _context.Members
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.RankLevel)
                .ThenByDescending(m => m.WinMatches)
                .ThenBy(m => m.TotalMatches)
                .Take(limit)
                .Select(m => new
                {
                    m.Id,
                    m.FullName,
                    m.RankLevel,
                    m.TotalMatches,
                    m.WinMatches,
                    WinRate = m.TotalMatches > 0 ? Math.Round((double)m.WinMatches / m.TotalMatches * 100, 1) : 0
                })
                .ToListAsync();

            return Ok(topMembers);
        }

        // =========================
        // GET: api/members/stats - Thống kê tổng quan
        // =========================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalMembers = await _context.Members.CountAsync();
            var activeMembers = await _context.Members.CountAsync(m => m.IsActive);
            var totalMatches = await _context.Members.SumAsync(m => m.TotalMatches);
            var avgRank = totalMembers > 0
                ? await _context.Members.Where(m => m.IsActive).AverageAsync(m => m.RankLevel)
                : 0;

            return Ok(new
            {
                totalMembers,
                activeMembers,
                inactiveMembers = totalMembers - activeMembers,
                totalMatches,
                averageRank = Math.Round(avgRank, 2)
            });
        }

        // =========================
        // PATCH: api/members/{id}/status - Chỉ Admin
        // =========================
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { message = $"Thành viên với id {id} không tồn tại" });

            member.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        // =========================
        // PATCH: api/members/{id}/rank - Chỉ Admin
        // =========================
        [HttpPatch("{id}/rank")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRank(int id, [FromBody] UpdateRankDto dto)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { message = $"Thành viên với id {id} không tồn tại" });

            member.RankLevel = dto.RankLevel;
            await _context.SaveChangesAsync();

            return Ok(member);
        }
    }

    // =========================
    // DTOs
    // =========================
    public class CreateMemberDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public double? RankLevel { get; set; }
        public string? UserId { get; set; }
    }

    public class UpdateMemberDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public double? RankLevel { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class UpdateRankDto
    {
        public double RankLevel { get; set; }
    }
}
