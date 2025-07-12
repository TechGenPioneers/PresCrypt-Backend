using MimeKit;
using MailKit.Net.Smtp;
using PresCrypt_Backend.PresCrypt.API.Dto;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit.Utils;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices
{
    public class PatientEmailService : IPatientEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PatientEmailService> _logger;

        public PatientEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = null;  // No logger provided here
        }

        public PatientEmailService(IConfiguration configuration, ILogger<PatientEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;  // Logger provided here
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


        public async Task SendRescheduleConfirmationEmailAsync(AppointmentRescheduleEmailDto request)
        {
            try
            {
                _logger.LogInformation("Starting email send for {Email}", request.Email);

                // Verify configuration values
                var emailHost = _configuration["EmailHost"] ?? throw new ArgumentNullException("EmailHost configuration missing");
                var emailPort = int.Parse(_configuration["EmailPort"] ?? "587");
                var emailUser = _configuration["EmailUserName"] ?? throw new ArgumentNullException("EmailUserName configuration missing");
                var emailPass = _configuration["EmailPassword"] ?? throw new ArgumentNullException("EmailPassword configuration missing");
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:7021";
                var frontendUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:3000";

                _logger.LogDebug("Using SMTP: {Host}:{Port}", emailHost, emailPort);

                // Create email message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailUser));
                email.To.Add(MailboxAddress.Parse(request.Email));
                email.Subject = "Your Appointment Has Been Rescheduled";

                var body = BuildEmailBody(request, baseUrl, frontendUrl);
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                // Configure and send email
                using var smtp = new SmtpClient();

                // Add event handlers for connection logging
                smtp.Connected += (sender, e) => _logger.LogDebug("Connected to SMTP server");
                smtp.Authenticated += (sender, e) => _logger.LogDebug("Authenticated with SMTP server");
                smtp.Disconnected += (sender, e) => _logger.LogDebug("Disconnected from SMTP server");

                await smtp.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(emailUser, emailPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email successfully sent to {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}",
                    request.Email, ex.Message);
                throw; // Re-throw to let caller handle
            }
        }

        private string BuildEmailBody(AppointmentRescheduleEmailDto request, string baseUrl, string frontendUrl)
        {
            var confirmationLink = $"{baseUrl}/api/appointments/reschedule-confirm?appointmentId={request.AppointmentId}";
            var rescheduleLink = $"{frontendUrl}/appointments/book?appointmentId={request.AppointmentId}";

            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>
                <div style='background-color: #094A4D; padding: 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>Appointment Rescheduled</h1>
                </div>
        
                <div style='padding: 20px;'>
                    <p>Dear {request.Name},</p>
            
                    <p>We wanted to inform you about a change to your upcoming appointment:</p>
            
                    <div style='background-color: #f8f9fa; border-left: 4px solid #094A4D; padding: 12px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>Original Date:</strong></p>
                        <p style='margin: 4px 0; font-size: 1.1em;'>
                            {request.OldDateTime:dddd, MMMM dd, yyyy} at {request.OldDateTime:h:mm tt}
                        </p>
                    </div>
            
                    <div style='background-color: #f0f8ff; border-left: 4px solid #4CAF50; padding: 12px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>New Date:</strong></p>
                        <p style='margin: 4px 0; font-size: 1.1em; font-weight: bold; color: #094A4D;'>
                            {request.NewDateTime:dddd, MMMM dd, yyyy} at {request.NewDateTime:h:mm tt}
                        </p>
                    </div>
            
                    <p>Please confirm if this new time works for you:</p>
            
                    <div style='margin: 24px 0; text-align: center;'>
                        <a href='{confirmationLink}' style='
                            background-color: #094A4D;
                            color: white;
                            padding: 12px 24px;
                            text-align: center;
                            text-decoration: none;
                            display: inline-block;
                            border-radius: 8px;
                            font-weight: bold;
                            margin-right: 16px;
                            transition: background-color 0.3s;'>
                            Confirm This Time
                        </a>
                
                        <a href='{rescheduleLink}' style='
                            background-color: #6c757d;
                            color: white;
                            padding: 12px 24px;
                            text-align: center;
                            text-decoration: none;
                            display: inline-block;
                            border-radius: 8px;
                            font-weight: bold;
                            transition: background-color 0.3s;'>
                            Choose Different Time
                        </a>
                    </div>
            
                    <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
            
                    <p>Best regards,<br/>
                    <strong>The PresCrypt Team</strong></p>
                </div>
            </div>";
        }

        public void SendOtpEmail(PatientOtpEmailDto request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailUserName"]));
            email.To.Add(MailboxAddress.Parse(request.Email));
            email.Subject = "Your PresCrypt OTP Code";

            var body = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Your Verification Code</h2>
                    <p>Use the following OTP to verify your booking:</p>
                    <h1 style='font-size: 48px; color: #4CAF50;'>{request.Otp}</h1>
                    <p>Do not share it with anyone.</p>
                    <p>Thank you,<br />The PresCrypt Team</p>
                </div>";

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration["EmailHost"], 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailUserName"], _configuration["EmailPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);

            _logger.LogInformation("OTP email sent to {Email}", request.Email);
        }
    }
}