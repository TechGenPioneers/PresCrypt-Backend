using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.HospitalServices;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Patient")]
    public class HospitalController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public HospitalController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetHospitalsGroupedByCity()
        {
            var result = await _hospitalService.GetHospitalsGroupedByCityAsync();
            return Ok(result);
        }
    }
}
