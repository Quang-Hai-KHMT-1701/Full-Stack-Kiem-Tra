using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;
using PCM.Api.Models.Sports;
using PCM.Api.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PCM.Api.Controllers
{
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
        // GET: api/challenges - Ai cũng xem được
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Challenges.Include(c => c.Participants).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(c => c.Type == type);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Type,
                    c.GameMode,
                    c.Status,
                    c.StartDate,
                    c.EndDate,
                    c.EntryFee,
                    c.PrizePool,
                    c.MaxParticipants,
                    c.Config_TargetWins,
                    c.CurrentScore_TeamA,
                    c.CurrentScore_TeamB,
                    ParticipantCount = c.Participants.Count,
                    c.CreatedById,
                    c.CreatedDate
                })
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
        // GET: api/challenges/{id} - Ai cũng xem được
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                    .ThenInclude(p => p.Member)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            return Ok(new
            {
                challenge.Id,
                challenge.Title,
                challenge.Type,
                challenge.GameMode,
                challenge.Status,
                challenge.StartDate,
                challenge.EndDate,
                challenge.EntryFee,
                challenge.PrizePool,
                challenge.MaxParticipants,
                challenge.Config_TargetWins,
                challenge.CurrentScore_TeamA,
                challenge.CurrentScore_TeamB,
                challenge.CreatedById,
                challenge.CreatedDate,
                Participants = challenge.Participants.Select(p => new
                {
                    p.Id,
                    p.MemberId,
                    MemberName = p.Member?.FullName,
                    p.Team,
                    p.EntryFeePaid,
                    p.Status,
                    p.JoinedDate
                })
            });
        }

        // =========================
        // POST: api/challenges - Chỉ Admin mới tạo
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateChallengeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var member = await _context.Members.FirstOrDefaultAsync(x => x.UserId == userId);

            var challenge = new Challenge
            {
                Title = dto.Title,
                Type = dto.Type ?? "MiniGame",
                GameMode = dto.GameMode ?? GameMode.FreeForAll,
                Status = "Open",
                StartDate = dto.StartDate ?? DateTime.Now,
                EndDate = dto.EndDate,
                EntryFee = dto.EntryFee ?? 0,
                PrizePool = dto.PrizePool ?? 0,
                MaxParticipants = dto.MaxParticipants ?? 20,
                Config_TargetWins = dto.Config_TargetWins ?? 5,
                CurrentScore_TeamA = 0,
                CurrentScore_TeamB = 0,
                CreatedById = member?.Id ?? 1,
                CreatedDate = DateTime.Now
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            return Ok(challenge);
        }

        // =========================
        // PUT: api/challenges/{id} - Chỉ Admin
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChallengeDto dto)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            if (!string.IsNullOrEmpty(dto.Title))
                challenge.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Type))
                challenge.Type = dto.Type;
            if (dto.GameMode.HasValue)
                challenge.GameMode = dto.GameMode.Value;
            if (!string.IsNullOrEmpty(dto.Status))
                challenge.Status = dto.Status;
            if (dto.StartDate.HasValue)
                challenge.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue)
                challenge.EndDate = dto.EndDate.Value;
            if (dto.EntryFee.HasValue)
                challenge.EntryFee = dto.EntryFee.Value;
            if (dto.PrizePool.HasValue)
                challenge.PrizePool = dto.PrizePool.Value;
            if (dto.MaxParticipants.HasValue)
                challenge.MaxParticipants = dto.MaxParticipants.Value;
            if (dto.Config_TargetWins.HasValue)
                challenge.Config_TargetWins = dto.Config_TargetWins.Value;

            await _context.SaveChangesAsync();
            return Ok(challenge);
        }

        // =========================
        // DELETE: api/challenges/{id} - Chỉ Admin
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            _context.Participants.RemoveRange(challenge.Participants);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa challenge" });
        }

        // =========================
        // POST: api/challenges/{id}/join - Ai đăng nhập cũng tham gia được
        // =========================
        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var member = await _context.Members.FirstOrDefaultAsync(x => x.UserId == userId);
            if (member == null)
                return BadRequest(new { message = "Không tìm thấy thông tin thành viên" });

            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            if (challenge.Status != "Open")
                return BadRequest(new { message = "Challenge không còn mở đăng ký" });

            if (challenge.Participants.Any(p => p.MemberId == member.Id))
                return BadRequest(new { message = "Bạn đã tham gia challenge này rồi" });

            if (challenge.Participants.Count >= challenge.MaxParticipants)
                return BadRequest(new { message = "Challenge đã đủ số người tham gia" });

            var participant = new Participant
            {
                ChallengeId = id,
                MemberId = member.Id,
                Team = "",
                EntryFeePaid = challenge.EntryFee == 0,
                EntryFeeAmount = challenge.EntryFee,
                Status = "Confirmed",
                JoinedDate = DateTime.Now
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tham gia challenge thành công", participant });
        }

        // =========================
        // POST: api/challenges/{id}/leave - Ai đăng nhập cũng rời được
        // =========================
        [HttpPost("{id}/leave")]
        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            var member = await _context.Members.FirstOrDefaultAsync(x => x.UserId == userId);
            if (member == null)
                return BadRequest(new { message = "Không tìm thấy thông tin thành viên" });

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ChallengeId == id && p.MemberId == member.Id);

            if (participant == null)
                return BadRequest(new { message = "Bạn chưa tham gia challenge này" });

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã rời khỏi challenge" });
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
                    MemberRank = p.Member.RankLevel,
                    p.Team,
                    p.EntryFeePaid,
                    p.EntryFeeAmount,
                    p.Status,
                    p.JoinedDate
                })
                .OrderBy(p => p.Team)
                .ThenByDescending(p => p.MemberRank)
                .ToListAsync();

            return Ok(participants);
        }

        // =========================
        // POST: api/challenges/{id}/auto-divide-teams - Chỉ Admin
        // =========================
        [HttpPost("{id}/auto-divide-teams")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AutoDivideTeams(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            var participants = await _context.Participants
                .Include(p => p.Member)
                .Where(p => p.ChallengeId == id)
                .OrderByDescending(p => p.Member.RankLevel)
                .ToListAsync();

            if (participants.Count < 2)
                return BadRequest(new { message = "Cần ít nhất 2 người để chia đội" });

            // Chia theo kiểu snake draft để cân bằng rank
            // Người rank cao nhất → Team A
            // Người rank thứ 2 → Team B
            // Người rank thứ 3 → Team B
            // Người rank thứ 4 → Team A
            // ...
            for (int i = 0; i < participants.Count; i++)
            {
                int round = i / 2;
                if (round % 2 == 0)
                {
                    participants[i].Team = (i % 2 == 0) ? "TeamA" : "TeamB";
                }
                else
                {
                    participants[i].Team = (i % 2 == 0) ? "TeamB" : "TeamA";
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã chia đội tự động theo Rank",
                teamA = participants.Where(p => p.Team == "TeamA").Select(p => new { p.MemberId, p.Member.FullName, p.Member.RankLevel }),
                teamB = participants.Where(p => p.Team == "TeamB").Select(p => new { p.MemberId, p.Member.FullName, p.Member.RankLevel })
            });
        }

        // =========================
        // POST: api/challenges/{id}/start - Chỉ Admin
        // =========================
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Start(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            if (challenge.Status != "Open")
                return BadRequest(new { message = "Chỉ có thể bắt đầu challenge ở trạng thái Open" });

            challenge.Status = "Ongoing";
            challenge.StartDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã bắt đầu challenge", challenge });
        }

        // =========================
        // POST: api/challenges/{id}/finish - Chỉ Admin
        // =========================
        [HttpPost("{id}/finish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Finish(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            challenge.Status = "Finished";
            challenge.EndDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã kết thúc challenge", challenge });
        }

        // =========================
        // POST: api/challenges/{id}/cancel - Chỉ Admin
        // =========================
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound(new { message = $"Challenge với id {id} không tồn tại" });

            challenge.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hủy challenge", challenge });
        }
    }

    // =========================
    // DTOs
    // =========================
    public class CreateChallengeDto
    {
        public string Title { get; set; } = "";
        public string? Type { get; set; }
        public GameMode? GameMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? EntryFee { get; set; }
        public decimal? PrizePool { get; set; }
        public int? MaxParticipants { get; set; }
        public int? Config_TargetWins { get; set; }
    }

    public class UpdateChallengeDto
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public GameMode? GameMode { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? EntryFee { get; set; }
        public decimal? PrizePool { get; set; }
        public int? MaxParticipants { get; set; }
        public int? Config_TargetWins { get; set; }
    }
}
