using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientAttachmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenMrsAttachmentService _attachmentService;

        public PatientAttachmentController(ApplicationDbContext context, OpenMrsAttachmentService attachmentService)
        {
            _context = context;
            _attachmentService = attachmentService;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetAttachmentsByPatientId(string patientId)
        {
            var patient = await _context.Patient
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound(new { Success = false, Message = "Patient not found." });
            }

            if (string.IsNullOrWhiteSpace(patient.OpenMrsId))
            {
                return BadRequest(new { Success = false, Message = "Patient does not have a valid OpenMRS ID." });
            }

            var result = await _attachmentService.GetObservationWithAttachmentAsync(patient.OpenMrsId);

            if (!result.Success)
            {
                return StatusCode(500, new { Success = false, Message = result.ErrorMessage });
            }

            return Ok(result);
        }
    }
}
