using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.API.DTOs;
using PresCrypt_Backend.PresCrypt.Core.Models; // Ensure this using statement points to your Models folder
using System;
using System.Linq;
using System.Threading.Tasks;

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

        // PUT: api/PatientProfile/{patientId} (Update existing patient profile)
        [HttpPut("{patientId}")]
        public async Task<IActionResult> UpdatePatientProfile(string patientId, [FromBody] UpdatePatientProfileDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(patientId))
                {
                    return BadRequest(new { Message = "Patient ID cannot be null or empty." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating patient profile for ID: {PatientId}", patientId);

                var patientToUpdate = await _dbContext.Patient.FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patientToUpdate == null)
                {
                    _logger.LogWarning("Patient not found with ID: {PatientId}", patientId);
                    return NotFound(new { Message = $"Patient with ID {patientId} not found." });
                }

                // Check for potential conflicts with email and NIC on other patients
                if (await _dbContext.Patient.AnyAsync(p => p.Email == dto.Email && p.PatientId != patientId))
                {
                    return Conflict(new { Message = "Another patient with this email address already exists." });
                }

                if (await _dbContext.Patient.AnyAsync(p => p.NIC == dto.Nic && p.PatientId != patientId))
                {
                    return Conflict(new { Message = "Another patient with this NIC already exists." });
                }

                // Split the full name into first and last name
                var nameParts = dto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                patientToUpdate.FirstName = nameParts.FirstOrDefault() ?? "";
                patientToUpdate.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                // Update basic properties
                patientToUpdate.DOB = dto.BirthDate;
                patientToUpdate.Gender = dto.Gender?.FirstOrDefault().ToString().ToUpper(); // "Male" -> "M"
                patientToUpdate.Email = dto.Email.ToLower().Trim();
                patientToUpdate.NIC = dto.Nic.Trim();
                patientToUpdate.Address = dto.Address.Trim();
                patientToUpdate.ContactNo = dto.Phone.Trim();
                patientToUpdate.UpdatedAt = DateTime.UtcNow;

                // Convert and update profile image from base64 if provided
                if (!string.IsNullOrEmpty(dto.ProfileImageBase64))
                {
                    try
                    {
                        patientToUpdate.ProfileImage = Convert.FromBase64String(dto.ProfileImageBase64);
                    }
                    catch (FormatException)
                    {
                        return BadRequest(new { Message = "Invalid base64 format for profile image." });
                    }
                }

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully updated patient profile with ID: {PatientId}", patientToUpdate.PatientId);

                // Return 204 No Content to indicate successful update
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating patient profile with ID: {PatientId}", patientId);
                return StatusCode(500, new { Message = "An internal error occurred while processing your request." });
            }
        }

        // GET: api/PatientProfile/{patientId}/basic (Get basic patient info)
        [HttpGet("{patientId}/basic")]
        public async Task<IActionResult> GetPatientBasicInfo(string patientId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(patientId))
                {
                    return BadRequest(new { Message = "Patient ID cannot be null or empty." });
                }

                _logger.LogInformation("Fetching basic patient info with ID: {PatientId}", patientId);

                var patient = await _dbContext.Patient
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient not found with ID: {PatientId}", patientId);
                    return NotFound(new { Message = $"Patient with ID {patientId} not found." });
                }

                var basicInfo = new
                {
                    patient.PatientId,
                    patient.FirstName,
                    patient.LastName,
                    Name = $"{patient.FirstName} {patient.LastName}".Trim(),
                    patient.DOB,
                    patient.Gender,
                    patient.Email,
                    patient.NIC,
                    patient.Address,
                    patient.ContactNo,
                    ProfileImage = patient.ProfileImage != null ? Convert.ToBase64String(patient.ProfileImage) : null,
                    patient.CreatedAt,
                    patient.UpdatedAt,
                    patient.LastLogin
                };

                _logger.LogInformation("Successfully retrieved basic patient info with ID: {PatientId}", patientId);
                return Ok(basicInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching basic patient info for ID: {PatientId}", patientId);
                return StatusCode(500, new { Message = "An internal error occurred while processing your request." });
            }
        }
    }
}