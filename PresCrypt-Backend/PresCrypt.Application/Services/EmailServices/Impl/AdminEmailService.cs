using Microsoft.AspNetCore.Http.HttpResults;
using MimeKit;
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

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                var subject = "Approval Notice";

                var styleBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #4CAF50;'>Registration Approved</h2>
                        <p>Dear Dr. {adminDoctorRequestDto.FirstName} {adminDoctorRequestDto.LastName},</p>

                       <p>We are pleased to inform you that your registration request has been 
                       <strong style='color: #4CAF50;'>approved</strong> by the admin team.</p>

                       <p>You can now access and utilize the features of the <strong>PresCrypt</strong> platform.</p>

                        <p>If you have any questions or need assistance, feel free to reach out to us.</p>

                       <p>Best regards,<br />
                          <strong>The Admin Team</strong><br />
                          PresCrypt</p>
                        </div>";

                var message = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = subject,
                    Body = styleBody,
                    IsBodyHtml = true
                };
                message.To.Add(adminDoctorRequestDto.Email);

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

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                string subject;
                string htmlBody;

                if (adminContactUsDto.SenderType == "patient")
                {
                    subject = "Response to Your Inquiry (PresCrypt)";

                    htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                         <h2 style='color: #4CAF50;'>Response to Your Inquiry</h2>
                         <p>Dear {adminContactUsDto.FirstName} {adminContactUsDto.LastName},</p>

                         <p>Thank you for reaching out to us.</p>

                         <p>We have reviewed your message. 
                           <strong style='color: #4CAF50;'>{adminContactUsDto.Topic}</strong></p>

                          <p><strong>Our response:</strong></p>
                             <p style='background-color: #f9f9f9; border-left: 4px solid #4CAF50; padding: 10px;'>
                           {adminContactUsDto.ReplyMessage}
                             </p>

                         <p>If you have any further questions, feel free to contact us.</p>

                         <p>Best regards,<br />
                          <strong>PresCrypt Admin Team</strong></p>
                        </div>";
                }
                else if (adminContactUsDto.SenderType == "doctor")
                {
                    subject = "Reply from Admin Panel (PresCrypt)";

                    htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #ffffff; color: #333333;'>
                      <h2 style='color: #4CAF50; margin-top: 0;'>Reply from PresCrypt Admin Panel</h2>
  
                          <p style='margin: 16px 0;'>Dear Dr. {adminContactUsDto.FirstName} {adminContactUsDto.LastName},</p>

                             <p style='margin: 16px 0;'>
                           We have received your inquiry. 
                           <strong style='color: #4CAF50;'>{adminContactUsDto.Topic}</strong>, and here is our response:
                              </p>

                             <div style='background-color: #f9f9f9; border-left: 4px solid #4CAF50; padding: 16px; margin: 20px 0;'>
                          {adminContactUsDto.ReplyMessage}
                               </div>

                             <p style='margin: 16px 0;'>If anything remains unclear or you need further assistance, don’t hesitate to reach out.</p>

                             <p style='margin: 24px 0 0;'>Sincerely,<br />
                             <strong>PresCrypt Admin Team</strong></p>
                                </div>";
                }
                else
                {
                    throw new InvalidOperationException("Invalid sender type. Must be 'patient' or 'doctor'.");
                }

                Debug.WriteLine($"Receiver Email: {adminContactUsDto.Email}, Topic: {adminContactUsDto.Topic}, ReplyMessage: {adminContactUsDto.ReplyMessage}, Port: {port}");

                var message = new MailMessage
                {
                    From = new MailAddress(email!),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(adminContactUsDto.Email);

                await smtpClient.SendMailAsync(message);

                Console.WriteLine("Reply email sent successfully.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"General error: {e.Message}");
                throw;
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

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                var subject = "Cancellation Notice";

                var htmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style=""color: #FF0000;"">Registration Request Rejected</h2>

                         <p>Dear {adminEmailDto.FirstName} {adminEmailDto.LastName},</p>

                        <p>We regret to inform you that your registration request has been 
                        <strong style='color: #d32f2f;'>rejected</strong> by the admin team.</p>

                        <p><strong>Reason for rejection:</strong></p>
                         <div style='background-color: #f9f9f9; border-left: 4px solid #d32f2f; padding: 12px; margin: 10px 0;'>
                              {adminEmailDto.Reason}
                         </div>

                         <p>If you believe this was a mistake or require further clarification, please feel free to contact us.</p>

                         <p>Best regards,<br />
                        <strong>The Admin Team</strong><br />
                         PresCrypt</p>
                          </div>";

                var message = new MailMessage
                {
                    From = new MailAddress(email!),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(adminEmailDto.Receptor);

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
                Console.WriteLine($"General error: {e.Message}");
                return "Error";
            }
        }


    }
}
