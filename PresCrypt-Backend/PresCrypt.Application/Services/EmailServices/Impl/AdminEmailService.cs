using Microsoft.AspNetCore.Http.HttpResults;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System;
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

        public async Task ApproveEmail(AdminDoctorRequestDto adminDoctorRequestDto)
        {
            Console.WriteLine($"Approve Email: {adminDoctorRequestDto}");
            try
            {
                var email = configuration.GetValue<string>("EmailUserName");
                var password = configuration.GetValue<string>("EmailPassword");
                var host = configuration.GetValue<string>("EmailHost");
                var port = configuration.GetValue<int>("EmailPort");

                Debug.WriteLine($"Email: {email}, Password: {password}, Host: {host}, Port: {port}");

                var smtpClient = new SmtpClient(host, port);


                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(email, password);

                var subject = "Approval Notice";
                var body = $"Dear Dr.{adminDoctorRequestDto.FirstName} {adminDoctorRequestDto.LastName},\n\n" +
                $"We are pleased to inform you that your registration request has been approved by the admin team.\n\n" +
                $"You can now access and utilize the features of the PresCrypt platform.\n\n" +
                $"If you have any questions or need assistance, feel free to reach out to us.\n\n" +
                $"Best regards,\n" +
                $"The Admin Team\n" +
                $"PresCrypt";

                var message = new MailMessage(email!, adminDoctorRequestDto.Email, subject, body);
                await smtpClient.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send email: {e.Message}");
            }
        }

        public async Task ReplyMsg(AdminContactUsDto adminContactUsDto)
        {
            try
            {
                var email = configuration.GetValue<string>("EmailUserName");
                var password = configuration.GetValue<string>("EmailPassword");
                var host = configuration.GetValue<string>("EmailHost");
                var port = configuration.GetValue<int>("EmailPort");

                Debug.WriteLine($"Email: {email}, Password: {password}, Host: {host}, Port: {port}");

                var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                string subject;
                string body;

                if (adminContactUsDto.SenderType == "patient")
                {
                    subject = "Response to Your Inquiry (PresCrypt)";
                    body = $"Dear {adminContactUsDto.FirstName} {adminContactUsDto.LastName},\n\n" +
                           $"Thank you for reaching out to us. We have reviewed your message regarding \"{adminContactUsDto.Topic}\".\n\n" +
                           $"Our response:\n{adminContactUsDto.ReplyMessage}\n\n" +
                           $"If you have any further questions, feel free to contact us.\n\n" +
                           $"Best regards,\nPresCrypt Admin Team";
                }
                else if (adminContactUsDto.SenderType == "doctor")
                {
                    subject = "Reply from Admin Panel (PresCrypt)";
                    body = $"Dear Dr. {adminContactUsDto.FirstName} {adminContactUsDto.LastName},\n\n" +
                           $"We have received your inquiry on \"{adminContactUsDto.Topic}\" and here is our reply:\n\n" +
                           $"{adminContactUsDto.ReplyMessage}\n\n" +
                           $"If anything remains unclear or you need assistance, don’t hesitate to reach out.\n\n" +
                           $"Sincerely,\nPresCrypt Admin Team";
                }
                else
                {
                    throw new InvalidOperationException("Invalid sender type. Must be 'patient' or 'doctor'.");
                }
                Debug.WriteLine($"Receiver Email: {adminContactUsDto.Email}, Topic: {adminContactUsDto.Topic}, ReplyMessage: {adminContactUsDto.ReplyMessage}, Port: {port}");
                var message = new MailMessage(email!, adminContactUsDto.Email, subject, body);
                await smtpClient.SendMailAsync(message);

                Console.WriteLine("Reply email sent successfully.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                throw; // Optionally re-throw to bubble up error to controller
            }
            catch (Exception e)
            {
                Console.WriteLine($"General error: {e.Message}");
                throw; // Optionally re-throw
            }
        }


        public async Task<string> SendEmail(AdminEmailDto adminEmailDto)
        {
            try
            {
                var email = configuration.GetValue<string>("EmailUserName");
                var password = configuration.GetValue<string>("EmailPassword");
                var host = configuration.GetValue<string>("EmailHost");
                var port = configuration.GetValue<int>("EmailPort");

                Debug.WriteLine($"Email: {email}, Password: {password}, Host: {host}, Port: {port}");

                var smtpClient = new SmtpClient(host, port);


                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(email, password);

                var subject = "Cancellation Notice";
                var body = $"Dear {adminEmailDto.FirstName} {adminEmailDto.LastName},\n\n" +
           $"We regret to inform you that your registration request has been rejected by the admin team.\n\n" +
           $"Reason for rejection:\n{adminEmailDto.Reason}\n\n" +
           $"If you believe this was a mistake or need further clarification, please feel free to contact us.\n\n" +
           $"Best regards,\n" +
           $"The Admin Team \n" +
           $"PresCrypt";
                var message = new MailMessage(email!, adminEmailDto.Receptor, subject, body);
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
