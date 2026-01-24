using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Core;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Members.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();
            return Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Member member)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Member member)
        {
            if (id != member.Id) return BadRequest();

            _context.Entry(member).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("top-ranking")]
        public async Task<IActionResult> GetTopRanking([FromQuery] int limit = 5)
        {
            var topMembers = await _context.Members
                .OrderByDescending(m => m.RankLevel)
                .ThenByDescending(m => m.WinMatches)
                .Take(limit)
                .Select(m => new
                {
                    m.Id,
                    m.FullName,
                    m.RankLevel,
                    m.TotalMatches,
                    m.WinMatches,
                    WinRate = m.TotalMatches > 0 ? (double)m.WinMatches / m.TotalMatches * 100 : 0
                })
                .ToListAsync();

            return Ok(topMembers);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalMembers = await _context.Members.CountAsync();
            var activeMembers = await _context.Members.CountAsync(m => m.IsActive);
            var totalMatches = await _context.Members.SumAsync(m => m.TotalMatches);

            return Ok(new
            {
                totalMembers,
                activeMembers,
                inactiveMembers = totalMembers - activeMembers,
                totalMatches
            });
        }

    }
}
