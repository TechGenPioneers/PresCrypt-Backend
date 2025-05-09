using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly ILogger<AdminDashboardController> _logger;
        public AdminDashboardController(IAdminDashboardService adminDashboardService, ILogger<AdminDashboardController> logger)
        {
            _adminDashboardService = adminDashboardService;
            _logger = logger;
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

        [HttpGet("GetAllNotifications")]
        public async Task<IActionResult> GetAllNotifications()
        {
            try
            {
                var notifications = await _adminDashboardService.GetNotifications();
                if (notifications == null || !notifications.Any())
                {
                    return NotFound("No notifications found.");
                }
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "An error occurred while retrieving notifications");
            }
        }

        [HttpPost("{notificationId}")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var response = await _adminDashboardService.MarkNotificationAsRead(notificationId);

            if (response == "Success")
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var response = await _adminDashboardService.MarkAllAsRead();

            if (response == "Success")
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


    }
}
