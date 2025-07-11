using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorReportController : ControllerBase
    {
        private readonly DoctorReportService _reportService;

        public DoctorReportController(DoctorReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("generate-reports")]
        public async Task<IActionResult> GenerateAppointmentReport([FromBody] DoctorReportDto request)
        {
            try
            {
                var pdfBytes = await _reportService.GenerateReportAsync(request);
                return File(pdfBytes, "application/pdf",
                    $"AppointmentReport_{request.DoctorId}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}