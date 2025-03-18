using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }   

        [HttpGet("appointments/{patientId}")]
        public async Task<IActionResult> GetAppointmentsForPatient(string patientId)
        {
            var appointements = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Select(a => new
                {
                   
                    a.Date,
                 
                    a.Status
                })
                .ToListAsync();
            if (appointements == null)
            {
                return NotFound();
            }

            return Ok(appointements);
        }
    }
}
