using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.DTOs.Matches;
using PCM.Api.Enums;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MatchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =====================
    // GET
    // =====================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var matches = await _context.Matches
            .Include(m => m.Team1_Player1)
            .Include(m => m.Team2_Player1)
            .ToListAsync();

        return Ok(matches);
    }

    // =====================
    // POST - Chỉ Referee hoặc Admin mới được tạo trận
    // =====================
    [HttpPost]
    [Authorize(Roles = "Admin,Referee")]
    public async Task<IActionResult> Create([FromBody] CreateMatchDto dto)
    {
        try
        {
            // Validate required fields
            if (dto.Team1_Player1Id == 0 || dto.Team2_Player1Id == 0)
            {
                return BadRequest(new { message = "Team1_Player1Id và Team2_Player1Id là bắt buộc" });
            }

            var match = new Match
            {
                ChallengeId = dto.ChallengeId,
                IsRanked = dto.IsRanked,
                MatchFormat = dto.MatchFormat,
                Date = dto.MatchDate ?? DateTime.Now,

                Team1_Player1Id = dto.Team1_Player1Id,
                Team1_Player2Id = dto.Team1_Player2Id,

                Team2_Player1Id = dto.Team2_Player1Id,
                Team2_Player2Id = dto.Team2_Player2Id,

                WinningSide = dto.WinningSide ?? "A"
            };

            _context.Matches.Add(match);

            // 🧠 Cập nhật thống kê và Rank
            await UpdateMemberStats(match);

            // 🏆 Cập nhật Challenge score nếu thuộc Challenge
            await UpdateChallengeScore(match);

            await _context.SaveChangesAsync();

            return Ok(match);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo trận đấu", error = ex.Message });
        }
    }

    private async Task UpdateMemberStats(Match match)
    {
        var players = new List<int>
        {
            match.Team1_Player1Id,
            match.Team2_Player1Id
        };

        if (match.Team1_Player2Id.HasValue)
            players.Add(match.Team1_Player2Id.Value);

        if (match.Team2_Player2Id.HasValue)
            players.Add(match.Team2_Player2Id.Value);

        var members = await _context.Members
            .Where(m => players.Contains(m.Id))
            .ToListAsync();

        foreach (var m in members)
        {
            m.TotalMatches++;

            bool isWinner = false;
            if ((match.WinningSide == "A" &&
                 (m.Id == match.Team1_Player1Id || m.Id == match.Team1_Player2Id))
                ||
                (match.WinningSide == "B" &&
                 (m.Id == match.Team2_Player1Id || m.Id == match.Team2_Player2Id)))
            {
                m.WinMatches++;
                isWinner = true;
            }

            // 🎯 Cập nhật RankLevel nếu IsRanked = true
            if (match.IsRanked)
            {
                if (isWinner)
                {
                    m.RankLevel += 0.1; // Thắng +0.1
                }
                else
                {
                    m.RankLevel = Math.Max(0, m.RankLevel - 0.1); // Thua -0.1, không âm
                }
            }
        }
    }

    /// <summary>
    /// Cập nhật điểm Challenge nếu match thuộc Challenge TeamBattle
    /// </summary>
    private async Task UpdateChallengeScore(Match match)
    {
        if (!match.ChallengeId.HasValue)
            return;

        var challenge = await _context.Challenges.FindAsync(match.ChallengeId.Value);
        if (challenge == null)
            return;

        // Chỉ xử lý TeamBattle
        if (challenge.GameMode != GameMode.TeamBattle)
            return;

        // Cập nhật điểm theo phe thắng
        if (match.WinningSide == "Team1")
        {
            challenge.CurrentScore_TeamA++;
        }
        else if (match.WinningSide == "Team2")
        {
            challenge.CurrentScore_TeamB++;
        }

        // Kiểm tra đạt mốc thắng -> Kết thúc Challenge
        if (challenge.Config_TargetWins > 0)
        {
            if (challenge.CurrentScore_TeamA >= challenge.Config_TargetWins ||
                challenge.CurrentScore_TeamB >= challenge.Config_TargetWins)
            {
                challenge.Status = "Finished";
            }
        }
    }

    // =====================
    // GET: api/matches/{id}
    // =====================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var match = await _context.Matches
            .Include(m => m.Team1_Player1)
            .Include(m => m.Team1_Player2)
            .Include(m => m.Team2_Player1)
            .Include(m => m.Team2_Player2)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null)
            return NotFound();

        return Ok(match);
    }

    // =====================
    // PUT: api/matches/{id}
    // =====================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateMatchDto dto)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null)
            return NotFound();

        match.ChallengeId = dto.ChallengeId;
        match.IsRanked = dto.IsRanked;
        match.MatchFormat = dto.MatchFormat;
        match.Team1_Player1Id = dto.Team1_Player1Id;
        match.Team1_Player2Id = dto.Team1_Player2Id;
        match.Team2_Player1Id = dto.Team2_Player1Id;
        match.Team2_Player2Id = dto.Team2_Player2Id;
        match.WinningSide = dto.WinningSide;

        await _context.SaveChangesAsync();
        return Ok(match);
    }

    // =====================
    // DELETE: api/matches/{id}
    // =====================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null)
            return NotFound();

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // =====================
    // GET: api/matches/member/{memberId}
    // =====================
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetByMember(int memberId)
    {
        var matches = await _context.Matches
            .Include(m => m.Team1_Player1)
            .Include(m => m.Team1_Player2)
            .Include(m => m.Team2_Player1)
            .Include(m => m.Team2_Player2)
            .Where(m => m.Team1_Player1Id == memberId ||
                        m.Team1_Player2Id == memberId ||
                        m.Team2_Player1Id == memberId ||
                        m.Team2_Player2Id == memberId)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        return Ok(matches);
    }

    // =====================
    // GET: api/matches/challenge/{challengeId}
    // =====================
    [HttpGet("challenge/{challengeId}")]
    public async Task<IActionResult> GetByChallenge(int challengeId)
    {
        var matches = await _context.Matches
            .Include(m => m.Team1_Player1)
            .Include(m => m.Team1_Player2)
            .Include(m => m.Team2_Player1)
            .Include(m => m.Team2_Player2)
            .Where(m => m.ChallengeId == challengeId)
            .ToListAsync();

        return Ok(matches);
    }

    // =====================
    // GET: api/matches/recent
    // =====================
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 10)
    {
        var matches = await _context.Matches
            .Include(m => m.Team1_Player1)
            .Include(m => m.Team1_Player2)
            .Include(m => m.Team2_Player1)
            .Include(m => m.Team2_Player2)
            .OrderByDescending(m => m.Id)
            .Take(limit)
            .ToListAsync();

        return Ok(matches);
    }
}
