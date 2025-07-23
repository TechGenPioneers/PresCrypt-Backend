using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Doctor")]
    public class DoctorNotificationsController : ControllerBase
    {
        private readonly IDoctorNotificationService _notificationService;
        private readonly ILogger<DoctorNotificationsController> _logger;

        public DoctorNotificationsController(IDoctorNotificationService notificationService,
            ILogger<DoctorNotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorNotifications(string doctorId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsForDoctorAsync(doctorId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for doctor {DoctorId}", doctorId);
                return StatusCode(500, "An error occurred while retrieving notifications");
            }
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(notificationId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return StatusCode(500, "An error occurred while marking notification as read");
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(notificationId);
                if (!result)
                {
                    return NotFound("Notification not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                return StatusCode(500, "An error occurred while deleting the notification");
            }
        }
    }
}