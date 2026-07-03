using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Username")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        public string Role { get; set; } = "User";

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public bool RememberMe { get; set; }

        // ================= RESET PASSWORD (OTP) =================

        // Mã OTP 6 số dùng để xác thực yêu cầu quên mật khẩu
        public string? OtpCode { get; set; }

        // Thời điểm hết hạn của OTP (mặc định 5 phút)
        public DateTime? OtpExpiry { get; set; }

        // Token dùng một lần để xác thực bước đặt lại mật khẩu
        // (chỉ được cấp SAU KHI xác thực OTP thành công)
        public string? ResetToken { get; set; }

        // Thời điểm hết hạn của token (mặc định 15 phút)
        public DateTime? ResetTokenExpiry { get; set; }
    }
}