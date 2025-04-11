using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPrescriptionServices;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorPrescriptionController : ControllerBase
    {
        private readonly IDoctorPrescriptionSubmitService _doctorPrescriptionSubmitService;

        public DoctorPrescriptionController(IDoctorPrescriptionSubmitService doctorPrescriptionSubmitService)
        {
            _doctorPrescriptionSubmitService = doctorPrescriptionSubmitService;
        }

        [HttpPost("submit-prescription")]
        public async Task<IActionResult> SubmitPrescription([FromForm] DoctorPrescriptionDto doctorPrescriptionDto)
        {
            if (doctorPrescriptionDto.PrescriptionFile == null && string.IsNullOrWhiteSpace(doctorPrescriptionDto.PrescriptionText))
            {
                return BadRequest("Either a prescription file or prescription text must be provided.");
            }

            var result = await _doctorPrescriptionSubmitService.SubmitPrescriptionAsync(doctorPrescriptionDto);

            // Pass the prescription data to OpenMRS (assumed implementation)
            var openMRSResult = await _doctorPrescriptionSubmitService.SendToOpenMRSAsync(doctorPrescriptionDto);

            if (!openMRSResult)
            {
                return StatusCode(500, "Error while sending data to OpenMRS.");
            }

            return Ok(result);
        }
    }
}
