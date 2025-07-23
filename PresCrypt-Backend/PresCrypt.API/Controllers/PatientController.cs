using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ApplicationDbContext _context;


        public PatientController(IPatientService patientService, ApplicationDbContext context)
        {
            _patientService = patientService;
            _context = context;

        }

       
        [HttpGet("appointments/{patientId}")]
        public async Task<IActionResult> GetAppointmentsForPatient(string patientId)
        {
            var appointments = await _patientService.GetAppointmentsForPatientAsync(patientId);

            if (appointments == null)
            {
                return NotFound(new { Message = "Patient not found." }); 
            }

            
            return Ok(appointments);
        }



        [HttpGet("profileImage/{patientId}")]
        public async Task<IActionResult> GetProfileImage(string patientId)
        {
            var (imageData, fileName) = await _patientService.GetProfileImageAsync(patientId);

            if (imageData == null || imageData.Length == 0)
            {
                // Return a default avatar image from wwwroot
                var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile.png");
                var defaultImage = await System.IO.File.ReadAllBytesAsync(defaultPath);

                return File(defaultImage, "image/jpeg", "default-avatar.jpg");
            }

            return File(imageData, "image/jpeg", fileName);
        }


        [HttpGet("profileNavbarDetails/{patientId}")]
        public async Task<IActionResult> GetPatientNavBarDetails(string patientId)
        {
            var patientDetails = await _patientService.GetPatientNavBarDetailsAsync(patientId);

            if (patientDetails == null)
            {
                return NotFound(new { Message = "Patient not found." });
            }

            return Ok(patientDetails);
        }

        [HttpPost("ContactUs")]
        public async Task<IActionResult> ContactUs([FromBody] PatientContactUsDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _patientService.AddInquiryAsync(dto);
            return Ok(new { message = "Inquiry submitted successfully" });
        }

        [HttpGet("id-by-email")]
        public async Task<IActionResult> GetPatientIdByEmail([FromQuery] string email)
        {
            var patientInfo = await _patientService.GetPatientIdAndStatusByEmailAsync(email);

            if (patientInfo == null)
            {
                return NotFound(new { message = "Patient not found for the provided email." });
            }

            return Ok(patientInfo);
        }

        [HttpPost("update-cancel-status/{patientId}")]
        public async Task<IActionResult> UpdateCancelStatus(string patientId)
        {
            try
            {
                await _patientService.UpdateCancelStatusAsync(patientId);
                return Ok(new { message = "Patient cancel status updated" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Something went wrong", details = ex.Message });
            }
        }



    }
}