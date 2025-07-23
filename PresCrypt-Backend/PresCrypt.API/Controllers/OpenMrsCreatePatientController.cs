using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OpenMrsCreatePatientController : ControllerBase
    {
        private readonly IOpenMrsPatientCreateService _openMrsPatientService;
        private readonly ILogger<OpenMrsCreatePatientController> _logger;

        public OpenMrsCreatePatientController(
            IOpenMrsPatientCreateService openMrsPatientService,
            ILogger<OpenMrsCreatePatientController> logger)
        {
            _openMrsPatientService = openMrsPatientService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a patient in OpenMRS system using the local patient data
        /// </summary>
        /// <param name="request">The patient creation request containing the patient ID</param>
        /// <returns>OpenMRS patient creation response with OpenMRS ID and identifier</returns>
        /// <response code="200">Patient successfully created in OpenMRS</response>
        /// <response code="400">Invalid request or patient not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("create")]
        [ProducesResponseType(typeof(OpenMrsPatientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OpenMrsPatientResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OpenMrsPatientResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OpenMrsPatientResponseDto>> CreatePatientInOpenMrs(
            [FromBody] OpenMrsPatientCreateDto request)
        {
            _logger.LogInformation("Received request to create patient in OpenMRS for PatientId: {PatientId}", request?.PatientId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for OpenMRS patient creation request");
                return BadRequest(new OpenMrsPatientResponseDto
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            if (string.IsNullOrWhiteSpace(request.PatientId))
            {
                _logger.LogWarning("Empty PatientId provided in OpenMRS patient creation request");
                return BadRequest(new OpenMrsPatientResponseDto
                {
                    Success = false,
                    Message = "PatientId is required"
                });
            }

            try
            {
                var result = await _openMrsPatientService.CreatePatientInOpenMrsAsync(request.PatientId.Trim());

                if (result.Success)
                {
                    _logger.LogInformation("Successfully created patient in OpenMRS. PatientId: {PatientId}, OpenMrsId: {OpenMrsId}",
                        request.PatientId, result.OpenMrsId);
                    return Ok(result);
                }

                _logger.LogWarning("Failed to create patient in OpenMRS. PatientId: {PatientId}, Message: {Message}",
                    request.PatientId, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreatePatientInOpenMrs for PatientId: {PatientId}", request.PatientId);
                return StatusCode(StatusCodes.Status500InternalServerError, new OpenMrsPatientResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred. Please try again later."
                });
            }
        }

        /// <summary>
        /// Gets the OpenMRS status for a specific patient
        /// </summary>
        /// <param name="patientId">The local patient ID</param>
        /// <returns>Patient's OpenMRS integration status</returns>
        /// <response code="200">Successfully retrieved patient OpenMRS status</response>
        /// <response code="404">Patient not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("status/{patientId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetPatientOpenMrsStatus(
            [FromRoute][Required] string patientId)
        {
            _logger.LogInformation("Received request to get OpenMRS status for PatientId: {PatientId}", patientId);

            if (string.IsNullOrWhiteSpace(patientId))
            {
                return BadRequest(new { Message = "PatientId is required" });
            }

            try
            {
                // This endpoint can be expanded to check actual OpenMRS status
                // For now, it provides information about the service
                return Ok(new
                {
                    PatientId = patientId.Trim(),
                    Message = "Use the POST /api/openmrscreatepatient/create endpoint to create or sync patient with OpenMRS",
                    Endpoints = new
                    {
                        CreatePatient = "/api/openmrscreatepatient/create",
                        CheckStatus = $"/api/openmrscreatepatient/status/{patientId}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPatientOpenMrsStatus for PatientId: {PatientId}", patientId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An internal server error occurred" });
            }
        }

        /// <summary>
        /// Health check endpoint for OpenMRS integration service
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetHealthStatus()
        {
            return Ok(new
            {
                Service = "OpenMRS Patient Integration",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// Gets OpenMRS integration configuration (without sensitive data)
        /// </summary>
        /// <returns>OpenMRS configuration information</returns>
        [HttpGet("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetConfiguration()
        {
            return Ok(new
            {
                OpenMrsBaseUrl = "http://localhost:80/openmrs/ws/rest/v1",
                IdentifierTypeUuid = "05a29f94-c0ed-11e2-94be-8c13b969e334",
                LocationUuid = "aff27d58-a15c-49a6-9beb-d30dcfc0c66e",
                Features = new[]
                {
                    "Patient Creation",
                    "Identifier Generation",
                    "Basic Authentication",
                    "Error Logging"
                }
            });
        }
    }
}