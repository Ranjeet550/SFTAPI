using System.Net.Mail;
using System.Net;

namespace SFT.Services
{
    public class EmailService
    {
        private readonly AppDbContext _context;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _configuration;

        public EmailService(AppDbContext context, ILoggerService logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public string SendEmail(string to, string subject, string body)
        {
            string senderEmail = _configuration["EmailSettings:Email"];
            string senderPassword = _configuration["EmailSettings:Password"];
            string smtpServer = _configuration["EmailSettings:Host"];
            int smtpPort = _configuration.GetValue<int>("EmailSettings:Port");

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            try
            {
                smtpClient.Send(mailMessage);
                return ("email sent");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}", $"{ex.Message}", "EmailService");
                return (ex.Message);
            }
        }
    }
}
