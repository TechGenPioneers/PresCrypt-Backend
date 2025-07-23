using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly ILogger<AdminDashboardController> _logger;
        public AdminDashboardController(IAdminDashboardService adminDashboardService, ILogger<AdminDashboardController> logger)
        {
            _adminDashboardService = adminDashboardService;
            _logger = logger;
        }

        //get all dashboard data (patient vists , today appointments , doctor count and patient count)
        [HttpGet("GetAllData")]
        public async Task<IActionResult> GetDashboardData([FromQuery] string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest("Username is required.");
            }

            try
            {
                var dashboardData = await _adminDashboardService.GetDashboardData(userName);

                if (dashboardData == null)
                {
                    return NotFound("Dashboard data not found.");
                }

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                // Optional: log the exception here
                return StatusCode(500, $"An error occurred while fetching dashboard data: {ex.Message}");
            }
        }


        //get all notifications
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

        //mark as read notification
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

        //mark all as read all the notifications
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
