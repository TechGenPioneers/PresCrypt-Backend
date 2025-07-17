using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using PresCrypt_Backend.PresCrypt.API.Dto;

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

}
