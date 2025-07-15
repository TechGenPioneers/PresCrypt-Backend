using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PatientPDFController : ControllerBase
    {
        private readonly IPDFService _pdfService;

        public PatientPDFController(IPDFService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost("generate")]
        public IActionResult GeneratePDF([FromBody] AppointmentPDFDetailsDto details)
        {
            var pdf = _pdfService.GeneratePDF(details);
            return File(pdf, "application/pdf", "PatientReport.pdf");
        }

        [HttpPost("Reports/Generate")]
        public async Task<IActionResult> GeneratePdf([FromBody] List<PatientAppointmentListDto> appointments)
        {
            if (appointments == null || !appointments.Any())
                return BadRequest("No data to generate PDF");

            var pdfBytes = await _pdfService.GeneratePdfAsync(appointments);

            return File(pdfBytes, "application/pdf", "AppointmentReport.pdf");
        }
    }
}
