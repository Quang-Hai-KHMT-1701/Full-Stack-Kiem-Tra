using System.ComponentModel.DataAnnotations;

namespace PCM.Api.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Họ tên đầy đủ (không bắt buộc khi đăng ký)
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
