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
        public ActionResult<DoctorDto> AddDoctor([FromBody] DoctorDto newDoctorDto)
        {
            if (newDoctorDto == null)
            {
                return BadRequest("Doctor details are required.");
            }

            Debug.WriteLine(newDoctorDto);

            return Ok(newDoctorDto);
        }
    }
}
