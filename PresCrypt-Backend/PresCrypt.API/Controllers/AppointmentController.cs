using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IConfiguration _configuration;
        private readonly IPatientEmailService _patientEmailService;
        private readonly IDoctorNotificationService _doctorNotificationService;
        private readonly ILogger<AppointmentsController> _logger;
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context, 
            IAppointmentService appointmentService,
            IConfiguration configuration,
            IDoctorNotificationService doctorNotificationService,
            IPatientEmailService patientEmailService,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _configuration = configuration;
            _appointmentService = appointmentService;
            _doctorNotificationService = doctorNotificationService;
            _patientEmailService = patientEmailService;
            _logger = logger;
        }

        [HttpGet("by-doctor/{doctorId}")]
        public async Task<IActionResult> GetAppointments(string doctorId, [FromQuery] string date = null)
        {
            try
            {
                DateOnly? dateFilter = null;
                if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsedDate))
                {
                    dateFilter = parsedDate;
                }

                var appointments = await _appointmentService.GetAppointmentsAsync(doctorId, dateFilter);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching appointments");
            }
        }

        [HttpGet("availability/{date}")]
        public async Task<IActionResult> GetAvailabilityByDate(string date, [FromQuery] string doctorId)
        {
            var availabilities = await _appointmentService.GetAvailabilityByDateAsync(date, doctorId);

            if (availabilities == null || !availabilities.Any())
            {
                return NotFound($"No availability found for the date: {date}.");
            }

            return Ok(availabilities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentSave dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentService.CreateAppointmentAsync(dto);
            return Ok(appointment);
        }


        [HttpGet("recent-by-doctor/{doctorId}")]
        public async Task<IActionResult> GetRecentAppointmentsByDoctor(string doctorId)
        {
            try
            {
                if (string.IsNullOrEmpty(doctorId))
                    return BadRequest("Doctor ID is required");

                var appointments = await _appointmentService.GetRecentAppointmentsByDoctorAsync(doctorId);

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                // Include the actual error message in the response
                return StatusCode(500, $"An error occurred while fetching recent appointments: {ex.Message}");
            }
        }

        [HttpPost("count-by-dates")]
        public async Task<IActionResult> GetAppointmentCounts([FromBody] AppointmentCountDto request)
        {
            var counts = await _appointmentService.GetAppointmentCountsAsync(request.DoctorId, request.Dates);

            // Convert the dictionary keys to string (yyyy-MM-dd) format
            var formattedCounts = counts.ToDictionary(
                kvp => kvp.Key.ToString("yyyy-MM-dd"),
                kvp => kvp.Value
            );

            return Ok(formattedCounts);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<PatientAppointmentListDto>>> GetAppointmentsByPatientId(string patientId)
        {
            var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            if (result == null || result.Count == 0)
                return NotFound("No appointments found for the given patient ID.");

            return Ok(result);
        }


        [HttpGet("available-hospitals")]
        public async Task<IActionResult> GetAvailableHospitals([FromQuery] string doctorId, [FromQuery] string date)
        {
            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(date))
                return BadRequest("Doctor ID and date are required.");

            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");

            var hospitals = await _appointmentService.GetAvailableHospitalsByDateAsync(parsedDate, doctorId);

            return Ok(hospitals);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);

            if (!result)
                return NotFound("Appointment not found");

            return NoContent();
        }

        [HttpPost("reschedule-appointments")]
        public async Task<IActionResult> RescheduleAppointments([FromBody] AppointmentIdsRequest request)
        {
            if (request?.AppointmentIds == null || !request.AppointmentIds.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "At least one appointment ID must be provided."
                });
            }

            try
            {
                var results = await _appointmentService.RescheduleAppointmentsAsync(request.AppointmentIds);

                // Check if all operations succeeded
                if (results.All(r => r.Success))
                {
                    return Ok(new ApiResponse<List<AppointmentRescheduleResultDto>>
                    {
                        Success = true,
                        Data = results,
                        Message = "All appointments were rescheduled successfully"
                    });
                }

                // Check if all operations failed
                if (results.All(r => !r.Success))
                {
                    return StatusCode(500, new ApiResponse<List<AppointmentRescheduleResultDto>>
                    {
                        Success = false,
                        Data = results,
                        Message = "Failed to reschedule all appointments"
                    });
                }

                // Partial success
                return StatusCode(207, new ApiResponse<List<AppointmentRescheduleResultDto>>
                {
                        Success = false,
                        Data = results,
                        Message = "Some appointments failed to reschedule"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointments");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while rescheduling appointments.",
                    Error = ex.Message
                });
            }
            
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Error { get; set; }
            public T Data { get; set; }
        }

        [HttpPost("{appointmentId}/cancel")]
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            try
            {
                var patientId = User.Claims.FirstOrDefault(c => c.Type == "patientId")?.Value;

                if (string.IsNullOrEmpty(patientId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Patient authentication required",
                        Code = "PATIENT_ID_MISSING"
                    });
                }

                await _appointmentService.CancelAppointmentAsync(appointmentId, patientId);

                _logger.LogInformation("Appointment cancelled - ID: {AppointmentId}, Patient: {PatientId}",
                    appointmentId, patientId);

                return Ok(new
                {
                    Success = true,
                    Message = "Appointment cancelled successfully",
                    Data = new { AppointmentId = appointmentId }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Appointment cancellation failed - ID: {AppointmentId}", appointmentId);

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Code = ex is InvalidOperationException ? "INVALID_OPERATION" : "CANCELLATION_FAILED",
                    AppointmentId = appointmentId
                });
            }


        }

        [HttpGet("Appointments/GetByDateRange")]
        public async Task<IActionResult> GetAppointmentsByDateRange([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate);
            return Ok(appointments);
        }
    }
}