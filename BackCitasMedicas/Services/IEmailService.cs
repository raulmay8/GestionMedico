using System.Net.Mail;
using System.Net;

namespace BackCitasMedicas.Services
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("no-reply@gestionmedico.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            smtpClient.Send(mailMessage);
        }
    }
}