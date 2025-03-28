using Azure.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDoctorController : ControllerBase
    {
        private readonly IAdminDoctorService _adminDoctorServices;
        public AdminDoctorController(IAdminDoctorService adminDoctorServices)
        {
            _adminDoctorServices = adminDoctorServices;
        }

        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorAvailabilityDto newDoctor)
        {
            if (newDoctor == null || newDoctor.Doctor == null || newDoctor.Availability == null)
            {
                return BadRequest("Doctor and availability details are required.");
            }

            //Debug.WriteLine(newDoctor.Doctor);

            //foreach (var slot in newDoctor.Availability)
            //{
            //    Debug.WriteLine($"Day: {slot.Day}, Start Time: {slot.StartTime}, End Time: {slot.EndTime}, Hospital: {slot.HospitalId}");
            //}

            var savedDoctor = await _adminDoctorServices.SaveDoctor(newDoctor.Doctor);
            Debug.WriteLine($"save : {savedDoctor}");

           if(savedDoctor == "Success")
            {
                return Created(savedDoctor,newDoctor.Doctor);
            }
            else
            {
                return StatusCode(500, "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _adminDoctorServices.GetAllDoctor();  // Ensure we await the task
            if (doctors == null || !doctors.Any())
                return NotFound("No doctors found.");
            return Ok(doctors);
        }


    }
}
