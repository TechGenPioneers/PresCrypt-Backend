using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.API.Hubs;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DoctorNotificationHub> _doctorHub;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
            _context = _context;
            _doctorHub = _doctorHub;
        }

        // GET: Retrieve appointments for a specific patient
        [HttpGet("appointments/{patientId}")]
        public async Task<IActionResult> GetAppointmentsForPatient(string patientId)
        {
            var appointments = await _patientService.GetAppointmentsForPatientAsync(patientId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "No appointments found for this patient." });
            }

            return Ok(appointments);
        }

        // GET: Retrieve profile image of a patient
        [HttpGet("profileImage/{patientId}")]
        public async Task<IActionResult> GetProfileImage(string patientId)
        {
            var (imageData, fileName) = await _patientService.GetProfileImageAsync(patientId);

            if (imageData == null || imageData.Length == 0)
            {
                return NotFound(new { Message = "Profile image not found or patient not found." });
            }

            return File(imageData, "image/jpeg", fileName);
        }

        [HttpGet("profileNavbarDetails/{patientId}")]
        public async Task<IActionResult> GetPatientNavBarDetails(string patientId)
        {
            var patientDetails = await _patientService.GetPatientNavBarDetailsAsync(patientId);

            if (patientDetails == null)
            {
                return NotFound(new { Message = "Patient not found." });
            }

            return Ok(patientDetails);
        }

        [HttpPost("ContactUs")]
        public async Task<IActionResult> ContactUs([FromBody] PatientContactUsDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _patientService.AddInquiryAsync(dto);
            return Ok(new { message = "Inquiry submitted successfully" });
        }

        [HttpGet("id-by-email")]
        public async Task<IActionResult> GetPatientIdByEmail([FromQuery] string email)
        {
            var patientId = await _patientService.GetPatientIdByEmailAsync(email);

            if (string.IsNullOrEmpty(patientId))
            {
                return NotFound(new { message = "Patient not found for the provided email." });
            }

            return Ok(new { patientId });
        }


    }
}

