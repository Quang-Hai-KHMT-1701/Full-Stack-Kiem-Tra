using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.DTOs.Bookings;
using PCM.Api.Models.Core;

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
        // GET: api/bookings
        // =====================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.Member)
                .ToListAsync();

            return Ok(bookings);
        }

        // =====================
        // POST: api/bookings
        // =====================
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            // ❌ Giờ không hợp lệ
            if (dto.StartTime >= dto.EndTime)
                return BadRequest("StartTime must be before EndTime");

            // ❌ Trùng lịch
            var conflict = await _context.Bookings.AnyAsync(b =>
                b.CourtId == dto.CourtId &&
                dto.StartTime < b.EndTime &&
                dto.EndTime > b.StartTime
            );

            if (conflict)
                return BadRequest("Court already booked in this time range");

            var booking = new Booking
            {
                CourtId = dto.CourtId,
                MemberId = dto.MemberId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Notes = dto.Notes,
                Status = (Enums.BookingStatus)1
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(booking);
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
                return NotFound();

            return Ok(booking);
        }

        // =====================
        // PUT: api/bookings/{id}
        // =====================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.CourtId = dto.CourtId;
            booking.MemberId = dto.MemberId;
            booking.StartTime = dto.StartTime;
            booking.EndTime = dto.EndTime;
            booking.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // =====================
        // DELETE: api/bookings/{id}
        // =====================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================
        // POST: api/bookings/{id}/cancel
        // =====================
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = Enums.BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // =====================
        // POST: api/bookings/{id}/confirm
        // =====================
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = Enums.BookingStatus.Confirmed;
            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // =====================
        // POST: api/bookings/{id}/reject
        // =====================
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = Enums.BookingStatus.Cancelled;
            booking.Notes = dto?.Reason;
            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // =====================
        // GET: api/bookings/available-slots
        // =====================
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] int? courtId)
        {
            var query = _context.Bookings.AsQueryable();

            if (courtId.HasValue)
                query = query.Where(b => b.CourtId == courtId.Value);

            var bookedSlots = await query
                .Where(b => b.StartTime.Date == date.Date)
                .Select(b => new { b.CourtId, b.StartTime, b.EndTime })
                .ToListAsync();

            return Ok(bookedSlots);
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
                .Where(b => b.StartTime.Date == date.Date)
                .ToListAsync();

            return Ok(bookings);
        }

        // =====================
        // GET: api/bookings/my-bookings
        // =====================
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int memberId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            return Ok(bookings);
        }
    }

}
