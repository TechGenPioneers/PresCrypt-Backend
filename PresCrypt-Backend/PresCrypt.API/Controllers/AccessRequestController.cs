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

            // If approved request exists and not expired (within 10 minutes)
            if (existingRequest != null && existingRequest.Status == "Approved" &&
                existingRequest.RequestDateTime.AddMinutes(10) > DateTime.UtcNow)
            {
                return BadRequest(new { success = false, message = "Access already granted and not expired." });
            }

            // STEP 2: Create new Access Request
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


    [HttpPost("respond-access-request")]
    public async Task<IActionResult> RespondToAccessRequest([FromBody] AccessRequestResponseDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.PatientId) || string.IsNullOrEmpty(dto.DoctorId))
            return BadRequest("Invalid request data");

        _logger.LogInformation("RespondToAccessRequest called with DoctorId: {DoctorId}, PatientId: {PatientId}, Accepted: {Accepted}",
            dto.DoctorId, dto.PatientId, dto.Accepted);

        var request = await _context.DoctorPatientAccessRequests
            .Where(r => r.DoctorId == dto.DoctorId && r.PatientId == dto.PatientId && r.Status == "Pending")
            .OrderByDescending(r => r.RequestDateTime)
            .FirstOrDefaultAsync();

        if (request == null)
        {
            _logger.LogWarning("No pending access request found for DoctorId: {DoctorId}, PatientId: {PatientId}", dto.DoctorId, dto.PatientId);
            return NotFound("Request not found");
        }

        request.Status = dto.Accepted ? "Approved" : "Denied";
        request.AccessExpiry = dto.Accepted ? DateTime.UtcNow.AddHours(1) : DateTime.MinValue;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Response recorded." });
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
