using System.Text.Json.Serialization;

namespace PCM.Api.DTOs.Matches
{
    public class CreateMatchDto
    {
        // Support both naming conventions
        [JsonPropertyName("challengeId")]
        public int? ChallengeId { get; set; }

        [JsonPropertyName("isRanked")]
        public bool IsRanked { get; set; }

        // Accept "format" from frontend (Singles/Doubles) or "matchFormat" (int)
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("matchFormat")]
        public int? MatchFormatValue { get; set; }

        // Computed MatchFormat - convert string to int if needed
        public int MatchFormat => MatchFormatValue ?? (Format?.ToLower() == "doubles" ? 2 : 1);

        // Support both camelCase and PascalCase with underscores
        [JsonPropertyName("team1Player1Id")]
        public int? Team1Player1IdCamel { get; set; }

        public int Team1_Player1Id
        {
            get => Team1Player1IdCamel ?? 0;
            set => Team1Player1IdCamel = value;
        }

        [JsonPropertyName("team1Player2Id")]
        public int? Team1Player2IdCamel { get; set; }

        public int? Team1_Player2Id
        {
            get => string.IsNullOrEmpty(Team1Player2IdCamel?.ToString()) ? null : Team1Player2IdCamel;
            set => Team1Player2IdCamel = value;
        }

        [JsonPropertyName("team2Player1Id")]
        public int? Team2Player1IdCamel { get; set; }

        public int Team2_Player1Id
        {
            get => Team2Player1IdCamel ?? 0;
            set => Team2Player1IdCamel = value;
        }

        [JsonPropertyName("team2Player2Id")]
        public int? Team2Player2IdCamel { get; set; }

        public int? Team2_Player2Id
        {
            get => string.IsNullOrEmpty(Team2Player2IdCamel?.ToString()) ? null : Team2Player2IdCamel;
            set => Team2Player2IdCamel = value;
        }

        [JsonPropertyName("winningSide")]
        public string WinningSide { get; set; } = "A";

        // Additional fields from frontend
        [JsonPropertyName("courtId")]
        public int? CourtId { get; set; }

        [JsonPropertyName("matchDate")]
        public DateTime? MatchDate { get; set; }

        [JsonPropertyName("team1Score")]
        public int? Team1Score { get; set; }

        [JsonPropertyName("team2Score")]
        public int? Team2Score { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }
}
