using PCM.Api.Models.Core;

namespace PCM.Api.Models.Sports
{
    public class Participant
    {
        public int Id { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public string Team { get; set; } = "A"; // A / B
        public bool EntryFeePaid { get; set; }
        public decimal EntryFeeAmount { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Joined";
        public ICollection<Participant> Participants { get; set; }
        = new List<Participant>();

    }


}
