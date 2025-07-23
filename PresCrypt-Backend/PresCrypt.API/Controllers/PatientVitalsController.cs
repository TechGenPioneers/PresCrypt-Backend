using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientVitalsController : ControllerBase
    {
        private readonly IOpenMrsService _openMrsService;
        private readonly ILogger<PatientVitalsController> _logger;

        public PatientVitalsController(
            IOpenMrsService openMrsService,
            ILogger<PatientVitalsController> logger)
        {
            _openMrsService = openMrsService;
            _logger = logger;
        }

        [HttpPost("patient/{patientId}/update-vitals")]
        public async Task<IActionResult> UpdatePatientVitals(string patientId, [FromBody] PatientVitalsDto vitalsDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Set the patientId from URL parameter
                vitalsDto.PatientId = patientId;

                var result = await _openMrsService.CreateObservationsAsync(vitalsDto);

                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Patient vitals updated successfully in OpenMRS",
                        PatientId = patientId
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = "Failed to update some or all vital signs in OpenMRS"
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid patient ID provided: {PatientId}", patientId);
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient vitals for patient {PatientId}", patientId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while updating patient vitals"
                });
            }
        }

        // Optional endpoint removed - only the update-vitals endpoint is needed for main functionality
    }
}
