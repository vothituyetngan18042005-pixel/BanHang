using BanHang.Data;
using BanHang.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BanHang.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

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
        public IActionResult Register(Account model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check Email
            var checkEmail = _context.Users
                .FirstOrDefault(x => x.Email == model.Email);

            if (checkEmail != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(model);
            }

            // Check Username
            var checkUsername = _context.Users
                .FirstOrDefault(x => x.Username == model.Username);

            if (checkUsername != null)
            {
                ViewBag.Error = "Username đã tồn tại";
                return View(model);
            }

            // Default Role
            if (string.IsNullOrEmpty(model.Role))
            {
                model.Role = "User";
            }

            // HASH PASSWORD
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
        public async Task<IActionResult> Login(Account model)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Username == model.Username ||
                    x.Email == model.Username);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    user.Password))
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
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(
            string oldPassword,
            string newPassword,
            string confirmPassword)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var username = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Check old password
            if (!BCrypt.Net.BCrypt.Verify(
                oldPassword,
                user.Password))
            {
                ViewBag.Error =
                    "Mật khẩu cũ không đúng";

                return View();
            }

            // Validate new password
            if (string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error =
                    "Vui lòng nhập mật khẩu mới";

                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error =
                    "Mật khẩu mới tối thiểu 6 ký tự";

                return View();
            }

            // Confirm password
            if (newPassword != confirmPassword)
            {
                ViewBag.Error =
                    "Xác nhận mật khẩu không khớp";

                return View();
            }

            // HASH NEW PASSWORD
            user.Password =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.SaveChanges();

            ViewBag.Success =
                "Đổi mật khẩu thành công";

            return View();
        }

        // ================= FORGOT PASSWORD =================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(
            string identity)
        {
            if (string.IsNullOrEmpty(identity))
            {
                ViewBag.Error =
                    "Vui lòng nhập Email hoặc SĐT";

                return View();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == identity ||
                    x.Phone == identity);

            if (user == null)
            {
                ViewBag.Error =
                    "Không tìm thấy tài khoản";

                return View();
            }   

            ViewBag.Success =
                "Tìm thấy tài khoản. Bạn có thể đặt lại mật khẩu.";

            return View();
        }

        // ================= ACCESS DENIED =================

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}