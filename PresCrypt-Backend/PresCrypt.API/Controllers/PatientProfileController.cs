using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PatientProfileController> _logger;

        public PatientProfileController(ApplicationDbContext dbContext, ILogger<PatientProfileController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET: api/PatientProfile/{patientId}
        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetPatientById(string patientId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(patientId))
                {
                    return BadRequest("Patient ID cannot be null or empty");
                }

                _logger.LogInformation("Fetching patient with ID: {PatientId}", patientId);

                var patient = await _dbContext.Patient
                    .Include(p => p.User) // Include User details since Email is foreign key
                    .Include(p => p.Appointments)
                    .Include(p => p.Notifications)
                    .AsNoTracking() // Better performance for read-only operations
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient not found with ID: {PatientId}", patientId);
                    return NotFound(new { Message = $"Patient with ID {patientId} not found" });
                }

                _logger.LogInformation("Successfully retrieved patient with ID: {PatientId}", patientId);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient with ID: {PatientId}", patientId);
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        // GET: api/PatientProfile (Get all patients - optional)
        [HttpGet]
        public async Task<IActionResult> GetAllPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var skip = (page - 1) * pageSize;

                var patients = await _dbContext.Patient
                    .Include(p => p.User)
                    .Include(p => p.Appointments)
                    .Include(p => p.Notifications)
                    .AsNoTracking()
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var totalCount = await _dbContext.Patient.CountAsync();

                var response = new
                {
                    Data = patients,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all patients");
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        // GET: api/PatientProfile/by-email/{email}
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetPatientByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Email cannot be null or empty");
                }

                // Validate email format
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email format");
                }

                _logger.LogInformation("Fetching patient with email: {Email}", email);

                var patient = await _dbContext.Patient
                    .Include(p => p.User)
                    .Include(p => p.Appointments)
                    .Include(p => p.Notifications)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Email == email);

                if (patient == null)
                {
                    _logger.LogWarning("Patient not found with email: {Email}", email);
                    return NotFound(new { Message = $"Patient with email {email} not found" });
                }

                _logger.LogInformation("Successfully retrieved patient with email: {Email}", email);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient with email: {Email}", email);
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        // GET: api/PatientProfile/{patientId}/basic (Get basic patient info without related data)
        [HttpGet("{patientId}/basic")]
        public async Task<IActionResult> GetPatientBasicInfo(string patientId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(patientId))
                {
                    return BadRequest("Patient ID cannot be null or empty");
                }

                _logger.LogInformation("Fetching basic patient info with ID: {PatientId}", patientId);

                var patient = await _dbContext.Patient
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient not found with ID: {PatientId}", patientId);
                    return NotFound(new { Message = $"Patient with ID {patientId} not found" });
                }

                // Return only basic patient info without related collections
                var basicInfo = new
                {
                    patient.PatientId,
                    patient.FirstName,
                    patient.LastName,
                    patient.DOB,
                    patient.Gender,
                    patient.Email,
                    patient.BloodGroup,
                    patient.NIC,
                    patient.Address,
                    patient.ContactNo,
                    patient.Status,
                    patient.ProfileImage, // Include profile image byte array
                    patient.CreatedAt,
                    patient.UpdatedAt,
                    patient.LastLogin
                };

                _logger.LogInformation("Successfully retrieved basic patient info with ID: {PatientId}", patientId);
                return Ok(basicInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching basic patient info with ID: {PatientId}", patientId);
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }
    }
}