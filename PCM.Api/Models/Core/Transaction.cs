using System.ComponentModel.DataAnnotations;

namespace PCM.Api.Models.Core
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// "income" hoặc "expense"
        /// </summary>
        [Required]
        public string Type { get; set; } = "income";

        /// <summary>
        /// ID của category (1-8)
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Tên category (để hiển thị)
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Ngày giao dịch
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID thành viên liên quan (nếu có)
        /// </summary>
        public int? MemberId { get; set; }

        /// <summary>
        /// ID booking liên quan (nếu có)
        /// </summary>
        public int? BookingId { get; set; }

        /// <summary>
        /// ID challenge liên quan (nếu có)
        /// </summary>
        public int? ChallengeId { get; set; }

        /// <summary>
        /// Người tạo giao dịch
        /// </summary>
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
