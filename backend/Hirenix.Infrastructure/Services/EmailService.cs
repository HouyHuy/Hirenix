using Hirenix.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Hirenix.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpAsync(string toEmail, string otpCode)
    {
        var subject = "Hirenix - Mã xác thực OTP";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 32px; background: #f8f9fa; border-radius: 12px;'>
                <h2 style='color: #2563eb; text-align: center;'>Hirenix</h2>
                <p style='text-align: center; color: #374151;'>Mã xác thực của bạn là:</p>
                <div style='text-align: center; margin: 24px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #1e40af; background: #dbeafe; padding: 12px 24px; border-radius: 8px;'>{otpCode}</span>
                </div>
                <p style='text-align: center; color: #6b7280; font-size: 14px;'>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                <p style='text-align: center; color: #6b7280; font-size: 12px;'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string otpCode)
    {
        var subject = "⚠️ Hirenix - Yêu cầu đặt lại mật khẩu";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 32px; background: #fff7ed; border-radius: 12px; border: 1px solid #fed7aa;'>
                <h2 style='color: #ea580c; text-align: center;'>🔐 Đặt lại mật khẩu</h2>
                <p style='text-align: center; color: #374151;'>Ai đó đã yêu cầu đặt lại mật khẩu cho tài khoản Hirenix của bạn.</p>
                <p style='text-align: center; color: #374151;'>Sử dụng mã bên dưới để xác nhận:</p>
                <div style='text-align: center; margin: 24px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #9a3412; background: #ffedd5; padding: 12px 24px; border-radius: 8px;'>{otpCode}</span>
                </div>
                <p style='text-align: center; color: #6b7280; font-size: 14px;'>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                <div style='background: #fef2f2; border-radius: 8px; padding: 12px; margin-top: 16px; border: 1px solid #fecaca;'>
                    <p style='text-align: center; color: #dc2626; font-size: 13px; margin: 0;'>
                        ⚠️ <strong>Nếu đây không phải là bạn</strong>, hãy đổi mật khẩu ngay lập tức để bảo vệ tài khoản.
                    </p>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtpSettings["SenderName"] ?? "Hirenix",
            smtpSettings["SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            smtpSettings["Host"] ?? "smtp.gmail.com",
            int.Parse(smtpSettings["Port"] ?? "587"),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            smtpSettings["Username"],
            smtpSettings["Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
