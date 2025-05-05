using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);

            if (!result)
                return NotFound("Appointment not found");

            return NoContent();
        }


            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");

            var hospitals = await _appointmentService.GetAvailableHospitalsByDateAsync(parsedDate, doctorId);

            return Ok(hospitals);
        }

        [HttpPost("reschedule")]
        public async Task<IActionResult> RescheduleAppointment([FromBody] AppointmentRescheduleDto dto)
        {
            try
            {
                var rescheduledCount = await _appointmentService.RescheduleAppointmentsAsync(dto);

                return Ok(new
                {
                    success = true,
                    rescheduledCount,
                    message = $"Successfully rescheduled {rescheduledCount} appointments."
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("No upcoming appointments"))
            {
                return Ok(new
                {
                    success = true,
                    rescheduledCount = 0,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    errorType = "ValidationError",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    errorType = "ServerError",
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}