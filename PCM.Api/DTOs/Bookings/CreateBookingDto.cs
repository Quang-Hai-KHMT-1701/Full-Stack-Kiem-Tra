namespace PCM.Api.DTOs.Bookings
{
    public class CreateBookingDto
    {
        public int CourtId { get; set; }
        public int MemberId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string? Notes { get; set; }
    }

}
