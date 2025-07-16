using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Doctor")]
    [EnableCors("AllowReactApp")]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDoctorService _doctorServices;

        public DoctorController(ApplicationDbContext context, IDoctorService doctorServices)
        {
            _context = context;
            _doctorServices = doctorServices;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<DoctorSearchDto>>> GetDoctors([FromQuery] SearchDoctorRequest request)
        {
            var doctors = await _doctorServices.GetDoctorAsync(
                request.Specialization,
                request.HospitalName,
                request.Name
            );

            return Ok(doctors);
        }

        [HttpGet("book/{doctorId}")] // Uses Mapster
        public async Task<ActionResult<List<DoctorBookingDto>>> GetDoctorBookedbyId(string doctorId)
        {
            var doctor = await _context.Doctor.FindAsync(doctorId);
            if (doctor is null)
            {
                return NotFound();
            }

            var response = doctor.Adapt<DoctorBookingDto>();
            return Ok(response);
        }

        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations()
        {
            var specializations = await _doctorServices.GetAllSpecializationsAsync();
            return Ok(specializations);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorServices.GetAllDoctor();
            return Ok(doctors);
        }

        [HttpGet("availability-by-name")]
        public async Task<IActionResult> GetDoctorAvailabilityByName([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Doctor name is required.");

            var results = await _doctorServices.GetDoctorAvailabilityByNameAsync(name);

            if (!results.Any())
                return NotFound("No availability found for the given doctor name.");

            return Ok(results);
        }

        // ✅ SearchDoctorRequest can be defined inside the same controller file
        public class SearchDoctorRequest : IValidatableObject
        {
            public string? Specialization { get; set; }
            public string? HospitalName { get; set; }
            public string? Name { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                bool hasName = !string.IsNullOrWhiteSpace(Name);
                bool hasSpecAndHospital = !string.IsNullOrWhiteSpace(Specialization) && !string.IsNullOrWhiteSpace(HospitalName);

                if (!hasName && !hasSpecAndHospital)
                {
                    yield return new ValidationResult(
                        "Provide either a doctor name, or both specialization and hospital name.",
                        new[] { nameof(Name), nameof(Specialization), nameof(HospitalName) }
                    );
                }
            }
        }

        [HttpGet("get-doctor-id")]
        public async Task<IActionResult> GetDoctorIdByUserName()
        {
            string? userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return BadRequest("User not authenticated");

            var doctor = await _context.Doctor
                .Where(d => d.Email == userName)
                .Select(d => new { d.DoctorId })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound("Doctor not found");

            return Ok(new { doctorId = doctor.DoctorId });
        }

        [HttpPost("request-patient-access")]
        public async Task<IActionResult> RequestPatientAccess([FromBody] DoctorAccessRequestDto dto)
        {
            try
            {
                var accessRequest = new DoctorPatientAccessRequest
                {
                    DoctorId = dto.DoctorId,
                    PatientId = dto.PatientId,
                    RequestDateTime = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.DoctorPatientAccessRequests.Add(accessRequest);

                var notification = new PatientNotifications
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = "AccessRequest",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PatientNotifications.Add(notification);

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Access request sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}
