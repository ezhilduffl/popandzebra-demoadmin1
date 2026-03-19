using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;
using PopZebra.Services;
using System.Security.Claims;

namespace PopZebra.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly EmailService _email;

        public AccountController(
            AppDbContext db,
            JwtService jwt,
            EmailService email)
        {
            _db = db;
            _jwt = jwt;
            _email = email;
        }

        // ─────────────────────────────────────────────
        // GET  /Account/Login
        // ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is { IsAuthenticated: true })
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // ─────────────────────────────────────────────
        // POST /Account/Login
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var normalizedEmail = email.Trim().ToLower();

            var user = await _db.AdminUsers
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail
                                       && x.Password == password);

            if (user is null)
            {
                ViewBag.Error = "Access Denied: Invalid Credentials";
                return View();
            }

            // Generate JWT and store in cookie claims
            var token = _jwt.GenerateToken(user.Email);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role,  "Admin"),
                new Claim("JwtToken",       token)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

            return RedirectToAction("Index", "Dashboard");
        }

        // ─────────────────────────────────────────────
        // GET  /Account/Logout
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // ─────────────────────────────────────────────
        // POST /Account/SendOtp
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email is required."
                    });

                var normalizedEmail = email.Trim().ToLower();

                var user = await _db.AdminUsers
                    .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

                if (user is null)
                    return BadRequest(new
                    {
                        success = false,
                        message = "No account found with this email."
                    });

                // Generate 5-digit OTP
                var otp = new Random().Next(10000, 99999).ToString();

                user.OtpCode = otp;
                user.OtpExpiry = DateTime.Now.AddMinutes(5);
                await _db.SaveChangesAsync();

                await _email.SendOtpEmailAsync(user.Email, otp);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to send OTP. Please try again.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────
        // POST /Account/VerifyOtp
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (dto is null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Otp))
                return BadRequest(new
                {
                    success = false,
                    message = "Email and OTP are required."
                });

            var user = await _db.AdminUsers
                .FirstOrDefaultAsync(x => x.Email == dto.Email.Trim().ToLower());

            if (user is null)
                return BadRequest(new
                {
                    success = false,
                    message = "User not found."
                });

            if (user.OtpCode != dto.Otp)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid OTP. Please check and try again."
                });

            if (user.OtpExpiry < DateTime.Now)
                return BadRequest(new
                {
                    success = false,
                    message = "OTP has expired. Please request a new one."
                });

            return Ok(new { success = true });
        }

        // ─────────────────────────────────────────────
        // POST /Account/ResetPassword
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto is null)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request."
                });

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new
                {
                    success = false,
                    message = "New password is required."
                });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new
                {
                    success = false,
                    message = "Password must be at least 6 characters."
                });

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new
                {
                    success = false,
                    message = "Passwords do not match."
                });

            var user = await _db.AdminUsers
                .FirstOrDefaultAsync(x => x.Email == dto.Email.Trim().ToLower());

            if (user is null)
                return BadRequest(new
                {
                    success = false,
                    message = "User not found."
                });

            if (user.OtpCode != dto.Otp)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid OTP."
                });

            if (user.OtpExpiry < DateTime.Now)
                return BadRequest(new
                {
                    success = false,
                    message = "OTP has expired. Please request a new one."
                });

            // Update password and clear OTP
            user.Password = dto.NewPassword;
            user.OtpCode = null;
            user.OtpExpiry = null;

            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    // ─────────────────────────────────────────────────
    // DTOs
    // ─────────────────────────────────────────────────
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}