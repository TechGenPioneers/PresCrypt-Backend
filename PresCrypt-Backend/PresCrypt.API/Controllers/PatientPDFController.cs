using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientPDFController : ControllerBase
    {
        private readonly IPDFService _pdfService;

        public PatientPDFController(IPDFService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpGet("generate")]
        public IActionResult GeneratePDF([FromBody] AppointmentPDFDetailsDto details)
        {
            var pdf = _pdfService.GeneratePDF(details);
            return File(pdf, "application/pdf", "PatientReport.pdf");
        }

    }
}
