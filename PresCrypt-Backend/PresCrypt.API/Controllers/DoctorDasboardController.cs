using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorDashboardController : ControllerBase
    {
        private readonly IDoctorDashboardService _doctorDashboardService;

        public DoctorDashboardController(IDoctorDashboardService doctorDashboardService)
        {
            _doctorDashboardService = doctorDashboardService;
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] string doctorId)
        {
            var stats = await _doctorDashboardService.GetDashboardStatsAsync(doctorId);
            return Ok(stats);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetDoctorProfile([FromQuery] string doctorId)
        {
            var profile = await _doctorDashboardService.GetDoctorProfileAsync(doctorId);
            return Ok(profile);
        }

    }
}
