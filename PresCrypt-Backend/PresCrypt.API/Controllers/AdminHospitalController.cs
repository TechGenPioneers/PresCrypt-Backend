using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminHospitalController : ControllerBase
    {
        private readonly IAdminHospital _hospitalService;

        public AdminHospitalController(IAdminHospital hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // POST: api/AdminHospital
        [HttpPost]
        public async Task<ActionResult<AdminHospitalDto>> AddHospital([FromBody] AdminHospitalDto hospitalDto)
        {
            var createdHospital = await _hospitalService.AddHospital(hospitalDto);

            if (createdHospital != null)
            {
                // Return HTTP 201 Created with the URI of the created resource
                return Created();
            }
            else
            {
                return StatusCode(500, "Error occurred while creating the hospital.");
            }
        }


        // GET: api/AdminHospital
        [HttpGet]
        public async Task<ActionResult<List<AdminHospitalDto>>> GetAllHospitals()
        {
            var hospitals = await _hospitalService.GetAllHospital();
            return Ok(hospitals);
        }

        // GET: api/AdminHospital/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminHospitalDto>> GetHospitalById(string id)
        {
            var hospital = await _hospitalService.GetHospitalById(id);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        // PUT: api/AdminHospital
        [HttpPut]
        public async Task<IActionResult> UpdateHospital([FromBody] AdminHospitalDto hospitalDto)
        {
            var success = await _hospitalService.UpdateHospital(hospitalDto);
            if (!success)
                return NotFound("Hospital not found.");

            return NoContent();
        }

        // DELETE: api/AdminHospital/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHospital(string id)
        {
            var success = await _hospitalService.DeleteHospital(id);
            if (!success)
                return NotFound("Hospital not found.");

            return NoContent();
        }
    }
}
