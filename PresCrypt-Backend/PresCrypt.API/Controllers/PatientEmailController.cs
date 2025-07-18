using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices;


namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientEmailController : ControllerBase
    {

        private readonly IPatientEmailService _patientEmailService;

        public PatientEmailController(IPatientEmailService patientEmailService)
        {
            _patientEmailService = patientEmailService;
        }

        [HttpPost]
        public IActionResult SendEmail(PatientAppointmentEmailDto request)
        {
            _patientEmailService.SendEmail(request);

            return Ok();
        }

        [HttpPost("reschedule")]
        public async Task<IActionResult> SendRescheduleEmail([FromBody] AppointmentRescheduleEmailDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.AppointmentId))
            {
                return BadRequest("Invalid request data.");
            }

            await _patientEmailService.SendRescheduleConfirmationEmailAsync(request);

            return Ok("Reschedule confirmation email sent successfully.");
        }

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] PatientOtpEmailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest("Email and OTP are required.");
            }

            _patientEmailService.SendOtpEmail(request);

            return Ok("OTP sent successfully.");
        }

        [HttpPost("send-cancellation-email")]
        public async Task<IActionResult> SendCancellationEmail([FromBody] AppointmentCancellationEmailDto dto)
        {
            try
            {
                await _patientEmailService.SendCancellationMessageEmailAsync(dto.Email, dto.PaymentMethod, dto.AppointmentDate, dto.AppointmentTime);
                return Ok(new { message = "Cancellation email sent successfully" });
            }
            catch (Exception ex)
            {
                // You can log the exception here if you want
                return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
            }
        }



    }
}
