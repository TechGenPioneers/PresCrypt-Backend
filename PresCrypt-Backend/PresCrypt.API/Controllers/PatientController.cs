using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ApplicationDbContext _context;
     

        public PatientController(IPatientService patientService,ApplicationDbContext context)
        {
            _patientService = patientService;
            _context = context;
            
        }

        // GET: Retrieve appointments for a specific patient
        [HttpGet("appointments/{patientId}")]
        public async Task<IActionResult> GetAppointmentsForPatient(string patientId)
        {
            var appointments = await _patientService.GetAppointmentsForPatientAsync(patientId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "No appointments found for this patient." });
            }

            return Ok(appointments);
        }

        // GET: Retrieve profile image of a patient
        [HttpGet("profileImage/{patientId}")]
        public async Task<IActionResult> GetProfileImage(string patientId)
        {
            var (imageData, fileName) = await _patientService.GetProfileImageAsync(patientId);

            if (imageData == null || imageData.Length == 0)
            {
                return NotFound(new { Message = "Profile image not found or patient not found." });
            }

            return File(imageData, "image/jpeg", fileName);
        }

        [HttpGet("profileNavbarDetails/{patientId}")]
        public async Task<IActionResult> GetPatientNavBarDetails(string patientId)
        {
            var patientDetails = await _patientService.GetPatientNavBarDetailsAsync(patientId);

            if (patientDetails == null)
            {
                return NotFound(new { Message = "Patient not found." });
            }

            return Ok(patientDetails);
        }

        [HttpPost("ContactUs")]
        public async Task<IActionResult> ContactUs([FromBody] PatientContactUsDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _patientService.AddInquiryAsync(dto);
            return Ok(new { message = "Inquiry submitted successfully" });
        }

        [HttpGet("id-by-email")]
        public async Task<IActionResult> GetPatientIdByEmail([FromQuery] string email)
        {
            var patientId = await _patientService.GetPatientIdByEmailAsync(email);

            if (string.IsNullOrEmpty(patientId))
            {
                return NotFound(new { message = "Patient not found for the provided email." });
            }

            return Ok(new { patientId });
        }
        //[HttpPost("respond-to-request")]
        //public async Task<IActionResult> RespondToAccessRequest([FromBody] PatientAccessResponseDto dto)
        //{
        //    var notification = await _context.PatientNotifications.FindAsync(dto.NotificationId);

        //    if (notification == null)
        //        return NotFound(new { message = "Notification not found" });

        //    var request = await _context.DoctorPatientAccessRequests
        //        .Where(r => r.DoctorId == dto.DoctorId && r.PatientId == notification.PatientId && r.Status == "Pending")
        //        .OrderByDescending(r => r.RequestDateTime)
        //        .FirstOrDefaultAsync();

        //    if (request == null)
        //        return NotFound(new { message = "Access request not found" });

        //    // Update Patient Notification as read
        //    notification.IsRead = true;

        //    if (dto.Accepted)
        //    {
        //        request.Status = "Approved";
        //        request.AccessExpiry = DateTime.UtcNow.AddHours(1);

        //        var doctorNotification = new DoctorNotification
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            DoctorId = dto.DoctorId,
        //            Type = "AccessGranted",
        //            Title = "Access Approved",
        //            Message = $"Patient {notification.PatientId} has granted access to their medical data.",
        //            IsRead = false,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        _context.DoctorNotifications.Add(doctorNotification);

        //        // Send real-time SignalR notification
        //        await _doctorHub.Clients.Group(dto.DoctorId).SendAsync("ReceiveNotification", new
        //        {
        //            id = doctorNotification.Id,
        //            title = doctorNotification.Title,
        //            message = doctorNotification.Message,
        //            type = doctorNotification.Type,
        //            createdAt = doctorNotification.CreatedAt,
        //            patientId = notification.PatientId // Include patientId for navigation
        //        });
        //    }
        //    else
        //    {
        //        request.Status = "Denied";
        //        var doctorNotification = new DoctorNotification
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            DoctorId = dto.DoctorId,
        //            Type = "AccessDenied",
        //            Title = "Access Denied",
        //            Message = $"Patient {notification.PatientId} has denied access to their medical data.",
        //            IsRead = false,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        _context.DoctorNotifications.Add(doctorNotification);

        //        // Send real-time SignalR notification
        //        await _doctorHub.Clients.Group(dto.DoctorId).SendAsync("ReceiveNotification", new
        //        {
        //            id = doctorNotification.Id,
        //            title = doctorNotification.Title,
        //            message = doctorNotification.Message,
        //            type = doctorNotification.Type,
        //            createdAt = doctorNotification.CreatedAt,
        //            patientId = notification.PatientId // Include patientId for navigation
        //        });
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Response saved." });
        //}
        //        [HttpPost("respond-access-request")]
        //        public async Task<IActionResult> RespondToAccessRequest([FromBody] AccessRequestResponseDto dto)
        //        {
        //            if (dto == null || string.IsNullOrEmpty(dto.PatientId) || string.IsNullOrEmpty(dto.DoctorId))
        //                return BadRequest("Invalid request data");

        //            var request = await _context.DoctorPatientAccessRequests
        //                .Where(r => r.DoctorId == dto.DoctorId && r.PatientId == dto.PatientId && r.Status == "Pending")
        //                .OrderByDescending(r => r.RequestDateTime)
        //                .FirstOrDefaultAsync();

        //            if (request == null)
        //                return NotFound("Request not found");

        //            if (dto.Accepted)
        //            {
        //                request.Status = "Approved";
        //                request.AccessExpiry = DateTime.UtcNow.AddHours(1);
        //            }
        //            else
        //            {
        //                request.Status = "Denied";
        //            }

        //            //// Mark any related patient notification as read (optional)
        //            //var notification = await _context.PatientNotifications
        //            //    .Where(n => n.PatientId == dto.PatientId && n.DoctorId == dto.DoctorId && !n.IsRead && n.Type == "Request")
        //            //    .OrderByDescending(n => n.CreatedAt)
        //            //    .FirstOrDefaultAsync();

        //            //if (notification != null)
        //            //{
        //            //    notification.IsRead = true;
        //            //}

        //            await _context.SaveChangesAsync();

        //            return Ok(new { success = true, message = "Response recorded." });

        //        }
        //        [HttpPost("request-patient-access")]
        //        public async Task<IActionResult> RequestPatientAccess([FromBody] DoctorAccessRequestDto dto)
        //        {
        //            try
        //            {
        //                var accessRequest = new DoctorPatientAccessRequest
        //                {
        //                    DoctorId = dto.DoctorId,
        //                    PatientId = dto.PatientId,
        //                    RequestDateTime = DateTime.UtcNow,
        //                    Status = "Pending"
        //                };

        //                _context.DoctorPatientAccessRequests.Add(accessRequest);

        //                var notification = new PatientNotifications
        //                {
        //                    PatientId = dto.PatientId,
        //                    DoctorId = dto.DoctorId,
        //                    Title = dto.Title,
        //                    Message = dto.Message,
        //                    Type = "Request",
        //                    IsRead = false,
        //                    CreatedAt = DateTime.UtcNow
        //                };

        //                _context.PatientNotifications.Add(notification);

        //                await _context.SaveChangesAsync();

        //                return Ok(new { success = true, message = "Access request sent successfully" });
        //            }
        //            catch (Exception ex)
        //            {
        //                return StatusCode(500, new { success = false, message = ex.Message });
        //            }
        //        }
        //        [HttpGet("validate-access")]
        //        public async Task<IActionResult> ValidateAccess([FromQuery] string doctorId, [FromQuery] string patientId)
        //        {
        //            var request = await _context.DoctorPatientAccessRequests
        //                .FirstOrDefaultAsync(r =>
        //                    r.DoctorId == doctorId &&
        //                    r.PatientId == patientId &&
        //                    r.Status == "Approved");

        //            if (request == null)
        //            {
        //                return Unauthorized(new { message = "Access not granted" });
        //            }

        //            if (request.AccessExpiry < DateTime.UtcNow)
        //            {
        //                return Unauthorized(new { message = "Access has expired" });
        //            }

        //            return Ok(new { message = "Access valid" });
        //        }
        //        [HttpGet("access-status")]
        //        public async Task<IActionResult> GetAccessStatus([FromQuery] string doctorId, [FromQuery] string patientId)
        //        {
        //            var request = await _context.DoctorPatientAccessRequests
        //                .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
        //                .OrderByDescending(r => r.RequestDateTime)
        //                .FirstOrDefaultAsync();

        //            if (request == null)
        //                return NotFound(new { status = "None" });

        //            if (request.Status == "Denied")
        //                return Ok(new { status = "Denied" });

        //            if (request.Status == "Approved" && request.AccessExpiry > DateTime.UtcNow)
        //                return Ok(new { status = "Approved", expiresAt = request.AccessExpiry });

        //            if (request.Status == "Approved" && request.AccessExpiry <= DateTime.UtcNow)
        //                return Ok(new { status = "Expired" });

        //            return Ok(new { status = request.Status });
        //        }
        [HttpPost("respond-access-request")]
        public async Task<IActionResult> RespondToAccessRequest([FromBody] AccessRequestResponseDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PatientId) || string.IsNullOrEmpty(dto.DoctorId))
                return BadRequest("Invalid request data");

            var request = await _context.DoctorPatientAccessRequests
                .Where(r => r.DoctorId == dto.DoctorId && r.PatientId == dto.PatientId && r.Status == "Pending")
                .OrderByDescending(r => r.RequestDateTime)
                .FirstOrDefaultAsync();

            if (request == null)
                return NotFound("Request not found");

            if (dto.Accepted)
            {
                request.Status = "Approved";
                request.AccessExpiry = DateTime.UtcNow.AddHours(1);
                request.GrantedAt = DateTime.UtcNow;
            }
            else
            {
                request.Status = "Denied";
               
            }

            // Mark any related patient notification as read
            var notification = await _context.PatientNotifications
                .Where(n => n.PatientId == dto.PatientId && n.DoctorId == dto.DoctorId && !n.IsRead && n.Type == "Request")
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            if (notification != null)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Response recorded successfully." });
        }
    }
}


