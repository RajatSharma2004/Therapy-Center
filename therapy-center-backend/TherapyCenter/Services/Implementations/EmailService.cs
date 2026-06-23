using System.Net;
using System.Net.Mail;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string toEmail, string otp, string purpose)
        {
            var smtpSection = _configuration.GetSection("SmtpSettings");

            var host = smtpSection["Host"]!;
            var port = int.Parse(smtpSection["Port"]!);

            var smtpLogin = smtpSection["Email"]!;
            var fromEmail = smtpSection["FromEmail"]!;

            var senderPassword = smtpSection["Password"]!;
            var enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl)
                ? ssl
                : true;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(
                    smtpLogin,
                    senderPassword),
                EnableSsl = enableSsl
            };

            var subject = $"Your OTP for {purpose}";
            var body = $"Your OTP is: {otp}\n\nIt will expire in 5 minutes.";

            using var message = new MailMessage(
                fromEmail,
                toEmail,
                subject,
                body);

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP ERROR: {ex}");
                throw;
            }
        }
}}
