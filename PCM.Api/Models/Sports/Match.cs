public class Match
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsRanked { get; set; }

    public int? ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }

    public int MatchFormat { get; set; } // 1: đơn, 2: đôi

    public int Team1_Player1Id { get; set; }
    public Member Team1_Player1 { get; set; } = null!;

    public int? Team1_Player2Id { get; set; }
    public Member? Team1_Player2 { get; set; }

    public int Team2_Player1Id { get; set; }
    public Member Team2_Player1 { get; set; } = null!;

    public int? Team2_Player2Id { get; set; }
    public Member? Team2_Player2 { get; set; }

    public string WinningSide { get; set; } = "A"; // A hoặc B
}
