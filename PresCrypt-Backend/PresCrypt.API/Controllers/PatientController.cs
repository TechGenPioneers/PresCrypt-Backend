using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
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

        
       //[HttpPost]
       // public async Task<IActionResult> AddPatient([FromForm] PatientCreateModel model)
       // {
       //     if (!ModelState.IsValid)
       //     {
       //         return BadRequest(ModelState);  // Return validation errors if any
       //     }

       //     byte[] imageData = null;

       //     // Handle Profile Image Upload (if present)
       //     if (model.ProfileImage != null && model.ProfileImage.Length > 0)
       //     {
       //         using (var ms = new MemoryStream())
       //         {
       //             await model.ProfileImage.CopyToAsync(ms);
       //             imageData = ms.ToArray();
       //         }
       //     }

       //     var patient = new Patient
       //     {
       //         PatientId = model.PatientId,
       //         FirstName = model.FirstName,
       //         LastName = model.LastName,
       //         DOB = model.DOB,
       //         Email = model.Email,
       //         BloodGroup = model.BloodGroup,
       //         NIC = model.NIC,
       //         ProfileImage = imageData,  // Save image as byte[]
       //         PasswordHash = model.PasswordHash,
       //         ContactNo = model.ContactNo,
       //         Status = "Active",  // Default status
       //         CreatedAt = DateTime.UtcNow,
       //         UpdatedAt = DateTime.UtcNow,
       //         LastLogin = null
       //     };

       //     await _context.Patient.AddAsync(patient);
       //     await _context.SaveChangesAsync();

       //     return Ok(new { Message = "Patient added successfully!", Patient = patient });
       // }
    }
}
