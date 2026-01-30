using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.DTOs.Bookings;
using PCM.Api.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================
        // GET: api/bookings - Admin xem tất cả, Member chỉ xem của mình
        // =====================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? courtId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdmin = role == "Admin";

            var query = _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .AsQueryable();

            // Nếu không phải Admin, chỉ xem booking của mình
            if (!isAdmin)
            {
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member != null)
                    query = query.Where(b => b.MemberId == member.Id);
            }

            if (courtId.HasValue)
                query = query.Where(b => b.CourtId == courtId.Value);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Enums.BookingStatus>(status, true, out var statusEnum))
                query = query.Where(b => b.Status == statusEnum);

            if (fromDate.HasValue)
                query = query.Where(b => b.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.EndTime <= toDate.Value);

            var total = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    b.Id,
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.MemberId,
                    MemberName = b.Member.FullName,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString(),
                    b.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                data = bookings,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // =====================
        // POST: api/bookings - Ai đăng nhập đều có thể đặt sân
        // =====================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdmin = role == "Admin";

            // Nếu không phải Admin, MemberId phải là của chính mình
            if (!isAdmin)
            {
                var currentMember = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (currentMember == null)
                    return BadRequest(new { message = "Không tìm thấy thông tin thành viên" });
                dto.MemberId = currentMember.Id;
            }

            // Kiểm tra giờ hợp lệ
            if (dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "Giờ bắt đầu phải trước giờ kết thúc" });

            // Kiểm tra sân tồn tại
            var court = await _context.Courts.FindAsync(dto.CourtId);
            if (court == null)
                return BadRequest(new { message = $"Sân với id {dto.CourtId} không tồn tại" });

            // Kiểm tra thành viên tồn tại
            var member = await _context.Members.FindAsync(dto.MemberId);
            if (member == null)
                return BadRequest(new { message = $"Thành viên với id {dto.MemberId} không tồn tại" });

            // Kiểm tra trùng lịch
            var conflict = await _context.Bookings.AnyAsync(b =>
                b.CourtId == dto.CourtId &&
                b.Status != Enums.BookingStatus.Cancelled &&
                dto.StartTime < b.EndTime &&
                dto.EndTime > b.StartTime
            );

            if (conflict)
                return BadRequest(new { message = "Sân đã được đặt trong khung giờ này" });

            var booking = new Booking
            {
                CourtId = dto.CourtId,
                MemberId = dto.MemberId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Notes = dto.Notes,
                Status = Enums.BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đặt sân thành công",
                booking = new
                {
                    booking.Id,
                    booking.CourtId,
                    CourtName = court.Name,
                    booking.MemberId,
                    MemberName = member.FullName,
                    booking.StartTime,
                    booking.EndTime,
                    Status = booking.Status.ToString(),
                    booking.Notes
                }
            });
        }

        // =====================
        // GET: api/bookings/{id}
        // =====================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            return Ok(new
            {
                booking.Id,
                booking.CourtId,
                CourtName = booking.Court.Name,
                booking.MemberId,
                MemberName = booking.Member.FullName,
                booking.StartTime,
                booking.EndTime,
                Status = booking.Status.ToString(),
                booking.Notes
            });
        }

        // =====================
        // PUT: api/bookings/{id} - Admin hoặc người đặt
        // =====================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdmin = role == "Admin";

            var booking = await _context.Bookings
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            // Chỉ Admin hoặc người đặt mới được sửa
            if (!isAdmin && booking.Member?.UserId != userId)
                return Forbid();

            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "Giờ bắt đầu phải trước giờ kết thúc" });

            // Kiểm tra trùng lịch nếu thay đổi thời gian
            var startTime = dto.StartTime ?? booking.StartTime;
            var endTime = dto.EndTime ?? booking.EndTime;
            var courtId = dto.CourtId ?? booking.CourtId;

            var conflict = await _context.Bookings.AnyAsync(b =>
                b.Id != id &&
                b.CourtId == courtId &&
                b.Status != Enums.BookingStatus.Cancelled &&
                startTime < b.EndTime &&
                endTime > b.StartTime
            );

            if (conflict)
                return BadRequest(new { message = "Sân đã được đặt trong khung giờ này" });

            if (dto.CourtId.HasValue)
                booking.CourtId = dto.CourtId.Value;
            if (dto.StartTime.HasValue)
                booking.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue)
                booking.EndTime = dto.EndTime.Value;
            if (dto.Notes != null)
                booking.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", booking });
        }

        // =====================
        // DELETE: api/bookings/{id} - Chỉ Admin
        // =====================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa booking" });
        }

        // =====================
        // POST: api/bookings/{id}/cancel - Admin hoặc người đặt
        // =====================
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdmin = role == "Admin";

            var booking = await _context.Bookings
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            // Chỉ Admin hoặc người đặt mới được hủy
            if (!isAdmin && booking.Member?.UserId != userId)
                return Forbid();

            booking.Status = Enums.BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hủy booking", booking });
        }

        // =====================
        // POST: api/bookings/{id}/confirm - Chỉ Admin
        // =====================
        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            booking.Status = Enums.BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xác nhận booking", booking });
        }

        // =====================
        // POST: api/bookings/{id}/reject - Chỉ Admin
        // =====================
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = $"Booking với id {id} không tồn tại" });

            booking.Status = Enums.BookingStatus.Cancelled;
            booking.Notes = dto?.Reason ?? "Bị từ chối bởi Admin";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối booking", booking });
        }

        // =====================
        // GET: api/bookings/available-slots - Xem các slot đã đặt trong ngày
        // =====================
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] int? courtId)
        {
            var query = _context.Bookings
                .Where(b => b.Status != Enums.BookingStatus.Cancelled)
                .AsQueryable();

            if (courtId.HasValue)
                query = query.Where(b => b.CourtId == courtId.Value);

            var bookedSlots = await query
                .Where(b => b.StartTime.Date == date.Date)
                .Select(b => new
                {
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString()
                })
                .OrderBy(b => b.CourtId)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            // Lấy danh sách sân
            var courts = await _context.Courts.Where(c => c.IsActive).ToListAsync();

            // Tạo danh sách slot từ 6:00 đến 22:00
            var slots = new List<object>();
            foreach (var court in courts)
            {
                if (courtId.HasValue && court.Id != courtId.Value)
                    continue;

                for (int hour = 6; hour < 22; hour++)
                {
                    var slotStart = date.Date.AddHours(hour);
                    var slotEnd = date.Date.AddHours(hour + 1);

                    var isBooked = bookedSlots.Any(b =>
                        b.CourtId == court.Id &&
                        slotStart < b.EndTime &&
                        slotEnd > b.StartTime);

                    slots.Add(new
                    {
                        courtId = court.Id,
                        courtName = court.Name,
                        startTime = slotStart,
                        endTime = slotEnd,
                        isAvailable = !isBooked
                    });
                }
            }

            return Ok(slots);
        }

        // =====================
        // GET: api/bookings/by-date
        // =====================
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .Where(b => b.StartTime.Date == date.Date && b.Status != Enums.BookingStatus.Cancelled)
                .Select(b => new
                {
                    b.Id,
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.MemberId,
                    MemberName = b.Member.FullName,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString(),
                    b.Notes
                })
                .OrderBy(b => b.CourtId)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            return Ok(bookings);
        }

        // =====================
        // GET: api/bookings/my-bookings
        // =====================
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
                return BadRequest(new { message = "Không tìm thấy thông tin thành viên" });

            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Where(b => b.MemberId == member.Id)
                .OrderByDescending(b => b.StartTime)
                .Select(b => new
                {
                    b.Id,
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString(),
                    b.Notes
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // =====================
        // GET: api/bookings/calendar - Lịch booking theo tuần/tháng
        // =====================
        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? courtId = null)
        {
            var query = _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .Where(b => b.StartTime >= startDate && b.EndTime <= endDate)
                .Where(b => b.Status != Enums.BookingStatus.Cancelled);

            if (courtId.HasValue)
                query = query.Where(b => b.CourtId == courtId.Value);

            var bookings = await query
                .Select(b => new
                {
                    b.Id,
                    b.CourtId,
                    CourtName = b.Court.Name,
                    b.MemberId,
                    MemberName = b.Member.FullName,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString(),
                    b.Notes
                })
                .ToListAsync();

            return Ok(bookings);
        }
    }

    // =====================
    // DTOs
    // =====================
    public class UpdateBookingDto
    {
        public int? CourtId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
