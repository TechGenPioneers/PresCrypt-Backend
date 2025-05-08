using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet ("GetAllData")]
        public async Task<IActionResult> GetDashboardData()
        {
            // Simulate fetching data from a service
            var dashboardData = await _adminDashboardService.GetDashboardData();
            if (dashboardData == null)
            {
                return NotFound("Dashboard data not found.");
            }
            return Ok(dashboardData);
        }
    }
}
