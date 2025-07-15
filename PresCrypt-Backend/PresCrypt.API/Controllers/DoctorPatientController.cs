using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorPatientController : ControllerBase
    {
        private readonly IDoctorPatientService _doctorPatientService;

        public DoctorPatientController(IDoctorPatientService doctorPatientService)
        {
            _doctorPatientService = doctorPatientService;
        }

        [HttpGet("patient-details/{doctorId}")]
        public async Task<IActionResult> GetPatientDetails(string doctorId, string type = "past")
        {
            var patients = await _doctorPatientService.GetPatientDetailsAsync(doctorId, type);

            if (patients == null || !patients.Any())
            {
                return NotFound("No patients found.");
            }

            return Ok(patients);
        }

    }
}