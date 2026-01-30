namespace PCM.Api.Models.Core
{
    public class News
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Tóm tắt ngắn gọn cho hiển thị danh sách
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// URL hình ảnh đại diện
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Ghim tin lên đầu
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Trạng thái: Draft, Published, Archived
        /// </summary>
        public string Status { get; set; } = "Published";

        /// <summary>
        /// Người tạo
        /// </summary>
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }
    }
}
