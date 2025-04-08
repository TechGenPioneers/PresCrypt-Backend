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

        [HttpGet("getAllPendingRequests")]
        public async Task<IActionResult> GetAllPendingRequest()
        {
            var pendingDoctors = await _adminDoctorRequestService.GetAllPendingDoctors();
            if (pendingDoctors == null || !pendingDoctors.Any())
                return NotFound("No Request found.");
            return Ok(pendingDoctors);
        }
        [HttpGet("getAllApprovedRequests")]
        public async Task<IActionResult> GetAllApprovedDoctors()
        {
            var approvedDoctors = await _adminDoctorRequestService.GetAllApprovedDoctors();
            if (approvedDoctors == null || !approvedDoctors.Any())
                return NotFound("No Request found.");
            return Ok(approvedDoctors);
        }
        [HttpGet("getAllRejectedRequests")]
        public async Task<IActionResult> GetAllRejectedRequest()
        {
            var rejectedDoctors = await _adminDoctorRequestService.GetAllRejectedDoctors();
            if (rejectedDoctors == null || !rejectedDoctors.Any())
                return NotFound("No Request found.");
            return Ok(rejectedDoctors);
        }
    }
}
