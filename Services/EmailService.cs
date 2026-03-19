using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PopZebra.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Pop and Zebra", _config["Email:From"]));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = "Pop and Zebra - Your OTP Code";

            var bodyHtml =
                "<div style='font-family:Segoe UI,sans-serif;max-width:520px;margin:auto;'>" +
                "  <div style='background:linear-gradient(135deg,#e94560,#c73652);" +
                "       padding:28px 32px;border-radius:14px 14px 0 0;text-align:center;'>" +
                "    <h2 style='color:#fff;margin:0;letter-spacing:1px;'>Pop &amp; Zebra</h2>" +
                "    <p style='color:rgba(255,255,255,.8);margin:4px 0 0;font-size:.9rem;'>Admin Panel</p>" +
                "  </div>" +
                "  <div style='background:#fff;padding:36px 32px;border:1px solid #eee;" +
                "       border-top:none;border-radius:0 0 14px 14px;'>" +
                "    <p style='color:#333;font-size:.95rem;'>Hello,</p>" +
                "    <p style='color:#555;'>You requested a password reset. " +
                "       Use the OTP code below to proceed:</p>" +
                "    <div style='background:#f8f9fe;padding:28px;text-align:center;" +
                "         border-radius:10px;margin:24px 0;border:2px dashed #e94560;'>" +
                "      <p style='margin:0 0 6px;font-size:.75rem;color:#999;" +
                "           text-transform:uppercase;letter-spacing:.1em;'>Your OTP Code</p>" +
                "      <h1 style='color:#e94560;letter-spacing:14px;font-size:42px;margin:0;'>" +
                otp +
                "      </h1>" +
                "    </div>" +
                "    <p style='color:#666;font-size:.9rem;'>This code is valid for <strong>5 minutes</strong>.</p>" +
                "    <p style='color:#999;font-size:.85rem;'>If you did not request this, please ignore this email.</p>" +
                "  </div>" +
                "  <p style='text-align:center;color:#bbb;font-size:.75rem;margin-top:14px;'>" +
                "     &copy; Pop and Zebra Admin Panel</p>" +
                "</div>";

            message.Body = new TextPart("html") { Text = bodyHtml };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Email:SmtpHost"]!,
                int.Parse(_config["Email:SmtpPort"]!),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["Email:Username"]!,
                _config["Email:Password"]!);

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}