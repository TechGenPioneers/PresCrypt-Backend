using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

[ApiController]
[Route("api/[controller]")]
public class AccessRequestController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccessRequestController> _logger;

    public AccessRequestController(ApplicationDbContext context, ILogger<AccessRequestController> logger)
    {
        _context = context;
        _logger = logger;
    }
    [HttpPost("request-patient-access")]
    public async Task<IActionResult> RequestPatientAccess([FromBody] DoctorAccessRequestDto dto)
    {
        try
        {
            // STEP 1: Check for existing Pending or Active Approved Request
            var existingRequest = await _context.DoctorPatientAccessRequests
                .Where(r => r.DoctorId == dto.DoctorId && r.PatientId == dto.PatientId)
                .OrderByDescending(r => r.RequestDateTime)
                .FirstOrDefaultAsync();

            // If pending request exists
            if (existingRequest != null && existingRequest.Status == "Pending")
            {
                return BadRequest(new { success = false, message = "A pending request already exists." });
            }

   
            var accessRequest = new DoctorPatientAccessRequest
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                RequestDateTime = DateTime.UtcNow,
                Status = "Pending"
            };
            _context.DoctorPatientAccessRequests.Add(accessRequest);

            // STEP 3: Add Notification
            var notification = new PatientNotifications
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                Title = dto.Title,
                Message = dto.Message,
                Type = "Request",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.PatientNotifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Access request sent successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


    [HttpPost("respond-to-access-request")]
    public async Task<IActionResult> RespondToAccessRequest([FromBody] AccessRequestResponseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var request = await _context.DoctorPatientAccessRequests
                .Where(r => r.PatientId == dto.PatientId && r.DoctorId == dto.DoctorId && r.Status == "Pending")
                .OrderByDescending(r => r.RequestDateTime)
                .FirstOrDefaultAsync();

            if (request == null)
            {
                return NotFound(new { success = false, message = "Access request not found or already handled." });
            }

            if (dto.Accepted)
            {
                request.Status = "Approved";
                request.GrantedAt = DateTime.UtcNow;
                //request.AccessExpiry = DateTime.UtcNow.AddHours(1);
            }
            else
            {
                request.Status = "Denied";
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Response recorded." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetAccessStatus(string doctorId, string patientId)
    {
        try
        {
            var request = await _context.DoctorPatientAccessRequests
                .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
                .OrderByDescending(r => r.RequestDateTime) // get most recent
                .FirstOrDefaultAsync();

            if (request == null)
                return NotFound("No access request found.");

            // Fix: Removed HasValue and Value as DateTime is a non-nullable struct
            var isExpired = request.AccessExpiry != DateTime.MinValue && DateTime.UtcNow > request.AccessExpiry;

            var result = new
            {
                request.RequestId,
                request.Status,
                request.AccessExpiry,
                request.GrantedAt,
                IsExpired = isExpired
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching access status: " + ex.Message);
        }
    }
    [HttpGet("check-access-status")]
    public async Task<IActionResult> CheckAccessStatus(string doctorId, string patientId)
    {
        try
        {
            var latestRequest = await _context.DoctorPatientAccessRequests
                .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
                .OrderByDescending(r => r.RequestDateTime)
                .FirstOrDefaultAsync();

            if (latestRequest == null)
            {
                return Ok(new { success = true, status = "NoRequest" });
            }

            // If it's approved, check if expired
            if (latestRequest.Status == "Approved")
            {
                var expiryTime = latestRequest.RequestDateTime.AddMinutes(10);

                if (DateTime.UtcNow > expiryTime)
                {
                    // Optional: Update the status to expired in DB
                    latestRequest.Status = "Expired";
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, status = "Expired" });
                }

                return Ok(new { success = true, status = "Approved" });
            }

            return Ok(new { success = true, status = latestRequest.Status }); // Pending / Denied
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


    [HttpPost("approve/{requestId}")]
    public async Task<IActionResult> ApproveAccess(int requestId)
    {
        var request = await _context.DoctorPatientAccessRequests.FindAsync(requestId);

        if (request == null)
            return NotFound("Request not found");

        request.Status = "Approved";
        request.GrantedAt = DateTime.Now;
        request.AccessExpiry = DateTime.Now.AddHours(1);

        await _context.SaveChangesAsync();
        return Ok("Access granted for 1 hour");
    }

}
