using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models; // Use your correct namespace heere
using PresCrypt_Backend.PresCrypt.API.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace PresCrypt_Backend.PresCrypt.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Patient,Doctor")]
    public class PatientNotificationController : ControllerBase
    {
        private readonly IHubContext<PatientNotificationHub> _hub;
        private readonly ApplicationDbContext _context;

        public PatientNotificationController(IHubContext<PatientNotificationHub> hub, ApplicationDbContext context)
        {
            _hub = hub;
            _context = context;
        }

        public class NotificationRequest
        {
            public string PatientId { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }

            public string Type { get; set; }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest req)
        {
            var notification = new PatientNotifications
            {
                PatientId = req.PatientId,
                Title = req.Title,
                Message = req.Message,
                IsRead = false,
                Type = req.Type,
                CreatedAt = DateTime.UtcNow
            };

            _context.PatientNotifications.Add(notification);
            await _context.SaveChangesAsync(); // Save to DB -> Now notification.Id is populated

            await _hub.Clients.User(req.PatientId).SendAsync("ReceiveNotification", new
            {
                id = notification.Id,         // Include id!
                title = notification.Title,
                message = notification.Message
            });

            return Ok(new { success = true });
        }


        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetNotifications(string patientId)
        {
            var notifications = await _context.PatientNotifications
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var enrichedNotifications = new List<object>();

            foreach (var notification in notifications)
            {
                if (notification.Type == "Request" && !string.IsNullOrEmpty(notification.DoctorId))
                {
                    var doctor = await _context.Doctor
                        .FirstOrDefaultAsync(d => d.DoctorId == notification.DoctorId);

                    enrichedNotifications.Add(new
                    {
                        notification.Id,
                        Title = $"Dr. {doctor?.FirstName + " "+ doctor.LastName?? "A doctor"} wants to access your prescription.",
                        Message = "Please respond to the access request by reviewing carefully.",
                        notification.CreatedAt,
                        notification.IsRead,
                        notification.Type,
                        notification.DoctorId
                    });
                }
                else
                {
                    enrichedNotifications.Add(new
                    {
                        notification.Id,
                        Title = notification.Title,
                        Message = notification.Message,
                        notification.CreatedAt,
                        notification.IsRead,
                        notification.Type
                    });
                }
            }

            return Ok(enrichedNotifications);
        }


        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] string id)
        {
            var notification = await _context.PatientNotifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}

