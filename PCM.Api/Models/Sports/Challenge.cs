using PCM.Api.Models.Sports;

public class Challenge
{
    internal DateTime CreatedDate;
    internal int CreatedById;

    public int Id { get; set; }

    public string? Title { get; set; }

    public DateTime StartDate { get; set; }

    public decimal EntryFee { get; set; }

    public decimal PrizePool { get; set; }

    public int MaxParticipants { get; set; }

    public string? Status { get; set; }

    // =====================
    // Navigation
    // =====================
    public ICollection<Participant> Participants { get; set; }
        = new List<Participant>();
}
