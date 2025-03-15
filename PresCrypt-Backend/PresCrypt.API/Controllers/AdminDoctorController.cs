using Azure.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDoctorController : ControllerBase
    {
        [HttpPost]
        public ActionResult<DoctorDto> AddDoctor([FromBody] DoctorAvailabilityDto newDoctor)
        {
            if (newDoctor == null || newDoctor.doctor == null || newDoctor.availability == null)
            {
                return BadRequest("Doctor and availability details are required.");
            }

            //Debug.WriteLine(newDoctorDto);
            Debug.WriteLine($"Doctor: {newDoctor.doctor}");
            Debug.WriteLine($"Availability: {newDoctor.availability}");

            return Ok(newDoctor);
        }
    }
}
