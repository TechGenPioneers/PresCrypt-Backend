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
    }
}