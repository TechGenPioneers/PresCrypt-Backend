using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("date/{date}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDisplayDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointmentsByDate(string date, [FromQuery] string doctorId)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(date, doctorId);

            // If the service returns null, return 404, otherwise 200
            return appointments == null ? NotFound($"No appointments found for the date: {date}.") : Ok(appointments);
        }


        [HttpGet("availability/{date}")]
        [ProducesResponseType(typeof(IEnumerable<AvailabilityDisplayDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailabilityByDate(string date, [FromQuery] string doctorId)
        {
            var availabilities = await _appointmentService.GetAvailabilityByDateAsync(date, doctorId);

            // If the service returns null, return 404, otherwise 200
            return availabilities == null ? NotFound($"No availability found for the date: {date}.") : Ok(availabilities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentSave dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentService.CreateAppointmentAsync(dto);
            return Ok(appointment);
        }
    }
}
