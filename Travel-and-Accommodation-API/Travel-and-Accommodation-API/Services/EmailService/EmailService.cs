using System.Net;
using System.Net.Mail;

namespace Travel_and_Accommodation_API.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmail(string message, string subject, string emailTo)
        {
            try
            {
                var email = _configuration["EmailConfig:EmailAddress"];
                var pw = _configuration["EmailConfig:Password"];

                var client = new SmtpClient(_configuration["EmailConfig:SmptClinet"], 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(email, pw)
                };
                await client.SendMailAsync(
                    new MailMessage(from: email,
                    to: emailTo,
                    subject,
                    message));
                _logger.LogInformation($"Email was sent successfully to {emailTo}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

    }
}
