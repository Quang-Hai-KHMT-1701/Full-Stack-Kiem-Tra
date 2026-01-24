namespace PCM.Api.DTOs.Matches
{
    public class CreateMatchDto
    {
        public int? ChallengeId { get; set; }
        public bool IsRanked { get; set; }

        public int MatchFormat { get; set; }

        public int Team1_Player1Id { get; set; }
        public int? Team1_Player2Id { get; set; }

        public int Team2_Player1Id { get; set; }
        public int? Team2_Player2Id { get; set; }

        public string WinningSide { get; set; } = "A";
    }

}
