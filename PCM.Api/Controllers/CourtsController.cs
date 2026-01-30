using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;
using PCM.Api.Models.Sports;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourtsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/courts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courts = await _context.Courts.ToListAsync();
            return Ok(courts);
        }

        // GET: api/courts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null)
                return NotFound();

            return Ok(court);
        }

        // POST: api/courts
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Court court)
        {
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = court.Id }, court);
        }

        // PUT: api/courts/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Court court)
        {
            if (id != court.Id)
                return BadRequest();

            _context.Entry(court).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/courts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null)
                return NotFound();

            _context.Courts.Remove(court);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/courts/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var activeCourts = await _context.Courts
                .Where(c => c.IsActive)
                .ToListAsync();

            return Ok(activeCourts);
        }
    }
}
