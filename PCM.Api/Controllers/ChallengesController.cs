using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Sports;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace PCM.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChallengesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/challenges
        // =========================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var challenges = await _context.Challenges
                .Include(c => c.Participants)
                .ToListAsync();

            return Ok(challenges);
        }

        // =========================
        // GET: api/challenges/{id}
        // =========================
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound();

            return Ok(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Challenge model)
        {
            // Try multiple claim types for UserId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var member = await _context.Members
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (member == null)
                return BadRequest($"Member not found for UserId: {userId}");

            model.CreatedById = member.Id;
            model.CreatedDate = DateTime.UtcNow;
            model.Status = model.Status ?? "Open";

            _context.Challenges.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // =========================
        // PUT: api/challenges/{id}
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Challenge model)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            challenge.Title = model.Title;
            challenge.StartDate = model.StartDate;
            challenge.MaxParticipants = model.MaxParticipants;
            challenge.EntryFee = model.EntryFee;
            challenge.PrizePool = model.PrizePool;

            await _context.SaveChangesAsync();
            return Ok(challenge);
        }

        // =========================
        // DELETE: api/challenges/{id}
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound();

            _context.Participants.RemoveRange(challenge.Participants);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // POST: api/challenges/{id}/join
        // =========================
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var member = await _context.Members.FirstOrDefaultAsync(x => x.UserId == userId);

            if (member == null)
                return BadRequest("Member not found");

            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound();

            if (challenge.Status != "Open")
                return BadRequest("Challenge is not open for registration");

            if (challenge.Participants.Any(p => p.MemberId == member.Id))
                return BadRequest("Already joined this challenge");

            if (challenge.Participants.Count >= challenge.MaxParticipants)
                return BadRequest("Challenge is full");

            var participant = new Participant
            {
                ChallengeId = id,
                MemberId = member.Id,
                JoinedDate = DateTime.UtcNow
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(participant);
        }

        // =========================
        // POST: api/challenges/{id}/leave
        // =========================
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            var member = await _context.Members.FirstOrDefaultAsync(x => x.UserId == userId);

            if (member == null)
                return BadRequest("Member not found");

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ChallengeId == id && p.MemberId == member.Id);

            if (participant == null)
                return BadRequest("Not joined this challenge");

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================
        // GET: api/challenges/{id}/participants
        // =========================
        [HttpGet("{id}/participants")]
        public async Task<IActionResult> GetParticipants(int id)
        {
            var participants = await _context.Participants
                .Include(p => p.Member)
                .Where(p => p.ChallengeId == id)
                .Select(p => new
                {
                    p.Id,
                    p.MemberId,
                    MemberName = p.Member.FullName,
                    p.Team,
                    p.JoinedDate
                })
                .ToListAsync();

            return Ok(participants);
        }

        // =========================
        // POST: api/challenges/{id}/auto-divide-teams
        // =========================
        [HttpPost("{id}/auto-divide-teams")]
        public async Task<IActionResult> AutoDivideTeams(int id)
        {
            var participants = await _context.Participants
                .Where(p => p.ChallengeId == id)
                .ToListAsync();

            if (participants.Count < 2)
                return BadRequest("Not enough participants");

            var shuffled = participants.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 0; i < shuffled.Count; i++)
            {
                shuffled[i].Team = i % 2 == 0 ? "A" : "B";
            }

            await _context.SaveChangesAsync();
            return Ok(participants);
        }

        // =========================
        // POST: api/challenges/{id}/start
        // =========================
        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            challenge.Status = "InProgress";
            await _context.SaveChangesAsync();

            return Ok(challenge);
        }

        // =========================
        // POST: api/challenges/{id}/finish
        // =========================
        [HttpPost("{id}/finish")]
        public async Task<IActionResult> Finish(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            challenge.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(challenge);
        }

        // =========================
        // POST: api/challenges/{id}/cancel
        // =========================
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            challenge.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(challenge);
        }
    }
}
