using System.ComponentModel.DataAnnotations;

namespace PCM.Api.Models.Core
{
    /// <summary>
    /// Danh mục Thu/Chi - Có thể thêm mới bởi Admin
    /// </summary>
    public class TransactionCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Loại: "income" (Thu) hoặc "expense" (Chi)
        /// </summary>
        [Required]
        public string Type { get; set; } = "income";

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Có đang sử dụng không
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
