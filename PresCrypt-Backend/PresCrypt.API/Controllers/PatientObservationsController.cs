using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PresCrypt_Backend.PresCrypt.Application.Services.OpenMrs_Services;

using PresCrypt_Backend.PresCrypt.Application.Services;



namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientObservationsController : ControllerBase
    {
        private readonly OPatientService _patientService;
        private readonly ILogger<PatientObservationsController> _logger;

        public PatientObservationsController(
            OPatientService OpatientService,
            ILogger<PatientObservationsController> logger)
        {
            _patientService = OpatientService;
            _logger = logger;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetPatientObservations(Guid patientId)
        {
            try
            {
                var observationsData = await _patientService.GetPatientObservations(patientId);
                return Ok(observationsData);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with OpenMRS API");
                return StatusCode(502, "Error communicating with OpenMRS API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving patient observations");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}