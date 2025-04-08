using Microsoft.AspNetCore.Http.HttpResults;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.Impl
{
    public class AdminEmailService : IAdminEmailService
    {
        private readonly IConfiguration configuration;

        public AdminEmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> SendEmail(AdminEmailDto adminEmailDto)
        {
            try
            {
                var email = configuration.GetValue<string>("EMAIL_CONFIGURATION:EMAIL");
                var password = configuration.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD");
                var host = configuration.GetValue<string>("EMAIL_CONFIGURATION:HOST");
                var port = configuration.GetValue<int>("EMAIL_CONFIGURATION:PORT");

                Debug.WriteLine($"Email: {email}, Password: {password}, Host: {host}, Port: {port}");

                var smtpClient = new SmtpClient(host, port);


                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(email, password);


                var message = new MailMessage(email!, adminEmailDto.Receptor, adminEmailDto.Subject, adminEmailDto.Body);
               await smtpClient.SendMailAsync(message);
                return "Success";
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                return "Send Failed";
            }
            catch (Exception e)
            {
                return "Error";
            }


        }

    }
}
