using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models; // Use your correct namespace heere
using PresCrypt_Backend.PresCrypt.API.Hubs;

namespace PresCrypt_Backend.PresCrypt.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                CreatedAt = DateTime.UtcNow
            };

            _context.PatientNotifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hub.Clients.User(req.PatientId).SendAsync("ReceiveNotification", new { Title = req.Title, Message = req.Message });

            return Ok(new { success = true });
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetNotifications(string patientId)
        {
            var notifications = await _context.PatientNotifications
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
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

