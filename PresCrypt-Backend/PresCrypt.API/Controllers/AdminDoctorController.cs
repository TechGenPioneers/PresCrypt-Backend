using Azure.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections;
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

            Debug.WriteLine(newDoctor.doctor);

            foreach (var slot in newDoctor.availability)
            {
                Debug.WriteLine($"Day: {slot.day}, Start Time: {slot.startTime}, End Time: {slot.endTime}");
            }

            return Ok(newDoctor);
        }
    }
}
