using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminPatientController : ControllerBase
    {
        private readonly IAdminPatientService _adminPatientService;
        public AdminPatientController(IAdminPatientService adminPatientService)
        {
            _adminPatientService = adminPatientService;
        }

        //get all patients details
        [HttpGet("GetAllPatients")]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var patients = await _adminPatientService.GetAllPatients();

                if (patients == null || !patients.Any())
                {
                    return NotFound("No patients found.");
                }

                return Ok(patients);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving patients: {e.Message}");
            }
        }

        //get patient details by id
        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetPatientByID(string patientId)
        {
            try
            {
                var patient = await _adminPatientService.GetPatientById(patientId);
                if(patient == null)
                {
                    return NotFound("No patient found.");
                }
                return Ok(patient);
            }catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving patient: {e.Message}");
            }
        }

        //update patient
        [HttpPatch]
        public async Task<IActionResult> UpdatePatient([FromBody] AdminUpdatePatientDto adminUpdatePatientDto)
        {
            if (adminUpdatePatientDto == null)
            {
                return BadRequest("Invalid patient data.");
            }
            try
            {
                var updatedPatient = await _adminPatientService.UpdatePatient(adminUpdatePatientDto);
                if (updatedPatient == "Success")
                {
                    return Ok(updatedPatient);
                }
                return NotFound("No patient found.");
               
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while updating the patient: {e.Message}");
            }
        }

    }
}
