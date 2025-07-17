using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminDoctorController : ControllerBase
    {
        private readonly IAdminDoctorService _adminDoctorServices;
        public AdminDoctorController(IAdminDoctorService adminDoctorServices)
        {
            _adminDoctorServices = adminDoctorServices;
        }

        //add new Doctor

        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorAvailabilityDto newDoctor)
        {
            if (newDoctor == null || newDoctor.Doctor == null || newDoctor.Availability == null)
            {
                return BadRequest("Doctor and availability details are required.");
            }

            var savedDoctor = await _adminDoctorServices.SaveDoctor(newDoctor);

            if (savedDoctor == "Success")
            {
                return Created(savedDoctor, newDoctor);
            }
            else
            {
                return StatusCode(500, "Error");
            }
        }

        //update doctor
        [HttpPatch]
        public async Task<IActionResult> UpdateDoctor([FromBody] DoctorAvailabilityDto updatedDoctor)
        {
            if (updatedDoctor == null || updatedDoctor.Doctor == null || updatedDoctor.Availability == null)
            {
                return BadRequest("Doctor and availability details are required.");
            }

            var updated = await _adminDoctorServices.UpdateDoctor(updatedDoctor);

            if(updated == "Success")
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode(500, "Error");
            }
        }

        //get all doctors
        [HttpGet("getAllDoctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _adminDoctorServices.GetAllDoctor();
            if (doctors == null || !doctors.Any())
                return NotFound("No doctors found.");
            return Ok(doctors);
        }

        //get doctor by id
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetDoctorByID(string doctorId)
        {
            var getDoctorAndAvailability = await _adminDoctorServices.getDoctorById(doctorId);

            if (getDoctorAndAvailability == null)
                return NotFound("No doctor found.");

            return Ok(getDoctorAndAvailability);
        }

        [HttpGet("getAllHospitals")]
        public async Task<IActionResult> getAllHospitals()
        {
            var hospitals = await _adminDoctorServices.getAllHospitals();
            if (hospitals == null || !hospitals.Any())
                return NotFound("No doctors found.");
            return Ok(hospitals);

        }

        //delete Doctor by id
        [HttpDelete ("{doctorId}")]
        public async Task<IActionResult> deleteDoctorById(string doctorId)
        {
            if(doctorId != null)
            {
                var deleted = await _adminDoctorServices.deleteDoctorById(doctorId);
                if(deleted== "Success")
                {
                    return Ok(deleted);
                }
                return BadRequest(deleted);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
