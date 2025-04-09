using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;

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
       
    }
}
