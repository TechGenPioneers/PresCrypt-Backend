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



    }
}
