using BanHang.Data;
using BanHang.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BanHang.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Thời gian hiệu lực OTP
        private const int OtpExpiryMinutes = 5;

        // Thời gian hiệu lực token đặt lại mật khẩu (sau khi xác thực OTP)
        private const int ResetTokenExpiryMinutes = 15;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Account model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var checkEmail = _context.Users
                .FirstOrDefault(x => x.Email == model.Email);

            if (checkEmail != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(model);
            }

            var checkUsername = _context.Users
                .FirstOrDefault(x => x.Username == model.Username);

            if (checkUsername != null)
            {
                ViewBag.Error = "Username đã tồn tại";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Role))
            {
                model.Role = "User";
            }

            model.Password =
                BCrypt.Net.BCrypt.HashPassword(model.Password);

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công";

            return RedirectToAction("Login");
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Account model)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Username == model.Username ||
                    x.Email == model.Username);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("Index", "Home");
        }

        // ================= LOGOUT =================

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }

        // ================= CHANGE PASSWORD =================

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(
            string oldPassword,
            string newPassword,
            string confirmPassword)
        {
            var username = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu mới";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới tối thiểu 6 ký tự";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp";
                return View();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();

            ViewBag.Success = "Đổi mật khẩu thành công";
            return View();
        }

        // ================= FORGOT PASSWORD (BƯỚC 1: NHẬP EMAIL/SĐT) =================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                ViewBag.Error = "Vui lòng nhập Email hoặc SĐT";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == identity ||
                    x.Phone == identity);

            // Thông báo chung cho cả 2 trường hợp tìm thấy / không tìm thấy
            // để tránh lộ thông tin tài khoản nào tồn tại (user enumeration).
            if (user == null)
            {
                ViewBag.Success =
                    "Nếu tài khoản tồn tại, mã OTP đã được gửi tới Email/SĐT của bạn.";
                return View();
            }

            // Sinh mã OTP 6 số ngẫu nhiên
            var otp = Random.Shared.Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(OtpExpiryMinutes);

            // Xóa reset token cũ (nếu có) vì đang bắt đầu luồng mới
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            // TODO: Trong thực tế, gửi mã OTP này qua Email/SMS thay vì hiển thị trực tiếp.
            // Ví dụ: await _smsService.SendOtpAsync(user.Phone, otp);
            TempData["DemoOtp"] = otp;

            // Chuyển sang bước nhập mã OTP, mang theo "identity" để tra lại đúng user
            return RedirectToAction("VerifyOtp", new { identity = identity });
        }

        // ================= VERIFY OTP (BƯỚC 2: NHẬP MÃ OTP) =================

        [HttpGet]
        public IActionResult VerifyOtp(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Identity = identity;

            // Chỉ dùng để hiển thị demo (không có SMTP/SMS thật)
            if (TempData["DemoOtp"] != null)
            {
                ViewBag.DemoOtp = TempData["DemoOtp"];
                TempData.Keep("DemoOtp"); // giữ lại để hiện khi người dùng bấm "Gửi lại mã"
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(string identity, string otpCode)
        {
            ViewBag.Identity = identity;

            if (string.IsNullOrWhiteSpace(identity))
            {
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrWhiteSpace(otpCode))
            {
                ViewBag.Error = "Vui lòng nhập mã OTP";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == identity ||
                    x.Phone == identity);

            if (user == null ||
                string.IsNullOrEmpty(user.OtpCode) ||
                user.OtpExpiry == null ||
                user.OtpExpiry < DateTime.Now)
            {
                ViewBag.Error = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
                return View();
            }

            if (user.OtpCode != otpCode.Trim())
            {
                ViewBag.Error = "Mã OTP không đúng. Vui lòng thử lại.";
                return View();
            }

            // OTP đúng: vô hiệu hóa OTP (dùng một lần), cấp Reset Token cho bước đổi mật khẩu
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.ResetToken = Guid.NewGuid().ToString("N");
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(ResetTokenExpiryMinutes);

            _context.SaveChanges();

            return RedirectToAction("ResetPassword", new { token = user.ResetToken });
        }

        // ================= RESET PASSWORD (BƯỚC 3: NHẬP MẬT KHẨU MỚI) =================

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !IsTokenValid(token, out _))
            {
                ViewBag.Error = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View("ResetPasswordInvalid");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(
            string token,
            string newPassword,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || !IsTokenValid(token, out var user))
            {
                ViewBag.Error = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View("ResetPasswordInvalid");
            }

            ViewBag.Token = token;

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới tối thiểu 6 ký tự";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp";
                return View();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";

            return RedirectToAction("Login");
        }

        private bool IsTokenValid(string token, out Account user)
        {
            user = _context.Users.FirstOrDefault(x => x.ResetToken == token);

            if (user == null || user.ResetTokenExpiry == null)
                return false;

            if (user.ResetTokenExpiry < DateTime.Now)
                return false;

            return true;
        }

        // ================= SETTINGS =================

        [HttpGet]
        [Authorize]
        public IActionResult Settings()
        {
            var username = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(Account model)
        {
            var username = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ViewBag.Error = "Vui lòng nhập họ tên";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.Error = "Vui lòng nhập Email";
                return View(user);
            }

            if (model.Email != user.Email)
            {
                var emailExists = _context.Users
                    .Any(x => x.Email == model.Email && x.Id != user.Id);

                if (emailExists)
                {
                    ViewBag.Error = "Email này đã được sử dụng bởi tài khoản khác";
                    return View(user);
                }
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.BirthDate = model.BirthDate;

            _context.SaveChanges();

            ViewBag.Success = "Cập nhật thông tin thành công";

            return View(user);
        }

        // ================= ACCESS DENIED =================

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}