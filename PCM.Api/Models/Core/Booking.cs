using PCM.Api.Enums;

namespace PCM.Api.Models.Core
{
    public class Booking
    {
        public int Id { get; set; }

        public int CourtId { get; set; }
        public Court Court { get; set; } 

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
