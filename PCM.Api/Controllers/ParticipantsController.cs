using Microsoft.AspNetCore.Mvc;
using PCM.Api.Data;
using PCM.Api.Models.Sports;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParticipantsController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var participants = await _context.Participants
            .Include(p => p.Member)
            .Include(p => p.Challenge)
            .ToListAsync();

        return Ok(participants);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinChallenge(JoinChallengeDto dto)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == dto.ChallengeId);

        if (challenge == null)
            return NotFound("Challenge not found");

        // ❌ Đã full
        if (challenge.Participants.Count >= challenge.MaxParticipants)
            return BadRequest("Challenge is full");

        // ❌ Join trùng
        if (challenge.Participants.Any(p => p.MemberId == dto.MemberId))
            return BadRequest("Member already joined");

        var participant = new Participant
        {
            ChallengeId = dto.ChallengeId,
            MemberId = dto.MemberId,
            EntryFeePaid = true,
            EntryFeeAmount = challenge.EntryFee
        };

        _context.Participants.Add(participant);

        // 💰 Cộng PrizePool
        challenge.PrizePool += challenge.EntryFee;

        await _context.SaveChangesAsync();

        return Ok(participant);
    }
}
