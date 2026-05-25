using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _apiKey;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Sendgrid:ApiKey"] ?? string.Empty;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var fromEmailAddress = _configuration["Sendgrid:FromEmail"] ?? "noreply@vocamon.online";
                var fromName = _configuration["Sendgrid:FromName"] ?? "Vocamon";
                var from = new EmailAddress(fromEmailAddress, fromName);
                var to = new EmailAddress(toEmail);
                var plainTextContent = "Vui lòng xem email này với trình duyệt hỗ trợ HTML.";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent to {ToEmail} successfully.", toEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to send email to {ToEmail}. Status code: {StatusCode}", toEmail, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending email to {ToEmail}", toEmail);
            }
        }
    }
}
