using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDoctorRequestController : ControllerBase
    {
        private readonly IAdminDoctorRequestService _adminDoctorRequestService;
        public AdminDoctorRequestController(IAdminDoctorRequestService adminDoctorRequestService)
        {
            _adminDoctorRequestService = adminDoctorRequestService;
        }

        [HttpGet("getAllDoctorRequest")]
        public async Task<IActionResult> GetAllPendingRequest()
        {
            var doctorRequests = await _adminDoctorRequestService.GetAllDoctorRequest();
            if (doctorRequests == null || !doctorRequests.Any())
                return NotFound("No Request found.");
            return Ok(doctorRequests);
        }

        //get doctor by id
        [HttpGet("{requestId}")]
        public async Task<IActionResult> GetDoctorByID(string requestId)
        {
            var getRequestAndAvailability = await _adminDoctorRequestService.getRequestByID(requestId);

            if (getRequestAndAvailability == null)
                return NotFound("No doctor found.");

            return Ok(getRequestAndAvailability);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateDoctor([FromBody] DoctorRequestRejectDto rejected)
        {
            if (rejected.RequestId == null || rejected.RequestId == null)
            {
                return BadRequest("null");
            }

            var updated = await _adminDoctorRequestService.RejectRequest(rejected);

            if (updated == "Success")
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode(500, "Error");
            }
        }
    }
}
