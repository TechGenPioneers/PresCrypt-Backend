using MimeKit;
using MailKit.Net.Smtp;
using PresCrypt_Backend.PresCrypt.API.Dto;
using MimeKit.Utils;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices
{
    public class PatientEmailService : IPatientEmailService
    {
        private readonly IConfiguration _configuration;

        public PatientEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(PatientAppointmentEmailDto request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("prescrypt.health@gmail.com"));
            email.To.Add(MailboxAddress.Parse(request.Receptor));
            email.Subject = request.Title;

            // Create the body builder
            var builder = new BodyBuilder
            {
                HtmlBody = request.Message
            };

            // Attach PDF if present
            if (request.Attachment != null && !string.IsNullOrEmpty(request.Attachment.Base64Content))
            {
                var pdfBytes = Convert.FromBase64String(request.Attachment.Base64Content);
                builder.Attachments.Add(request.Attachment.FileName, pdfBytes, ContentType.Parse(request.Attachment.ContentType));
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration["EmailHost"], 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailUserName"], _configuration["EmailPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
