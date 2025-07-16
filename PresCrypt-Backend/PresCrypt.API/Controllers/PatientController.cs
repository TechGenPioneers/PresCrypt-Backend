using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
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


        
    }
}

