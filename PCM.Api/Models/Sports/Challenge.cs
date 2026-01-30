using PCM.Api.Models.Sports;
using PCM.Api.Enums;

public class Challenge
{
    public int Id { get; set; }

    public string? Title { get; set; }

    /// <summary>
    /// Loại giải: Tournament, MiniGame
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Chế độ chơi: TeamBattle, RoundRobin, FreeForAll
    /// </summary>
    public GameMode GameMode { get; set; } = GameMode.FreeForAll;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal EntryFee { get; set; }

    public decimal PrizePool { get; set; }

    public int MaxParticipants { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Số trận cần thắng để kết thúc (TeamBattle mode)
    /// </summary>
    public int Config_TargetWins { get; set; } = 5;

    /// <summary>
    /// Điểm hiện tại của Team A
    /// </summary>
    public int CurrentScore_TeamA { get; set; } = 0;

    /// <summary>
    /// Điểm hiện tại của Team B
    /// </summary>
    public int CurrentScore_TeamB { get; set; } = 0;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public int CreatedById { get; set; }

    // =====================
    // Navigation
    // =====================
    public ICollection<Participant> Participants { get; set; }
        = new List<Participant>();
}
