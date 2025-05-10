using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientProfileController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public PatientProfileController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetPatientProfileByID(Guid id)
        {
            var patientProfile = dbContext.PatientProfiles.Find(id);

            if(patientProfile is null)
            {
                return NotFound();

            }
            return Ok(patientProfile);
            

        }
    }
}
