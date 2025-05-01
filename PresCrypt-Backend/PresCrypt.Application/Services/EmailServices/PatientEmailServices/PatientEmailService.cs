using Microsoft.AspNetCore.Identity;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;
using MimeKit;
using MailKit.Net.Smtp;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices
{
    public class PatientEmailService : IPatientEmailService
    {

        public readonly IConfiguration _configuration;

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
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = request.Message };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailHost").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);

            smtp.Authenticate(_configuration.GetSection("EmailUserName").Value, _configuration.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
