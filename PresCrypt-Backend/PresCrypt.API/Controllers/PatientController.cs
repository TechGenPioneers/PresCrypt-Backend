using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;

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

        // GET: Retrieve appointments for a specific patient
        [HttpGet("appointments/{patientId}")]
        public async Task<IActionResult> GetAppointmentsForPatient(string patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Select(a => new
                {
                    a.Date,
                    a.Status
                })
                .ToListAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound(new { Message = "No appointments found for this patient." });
            }

            return Ok(appointments);
        }

        // GET: Retrieve profile image of a patient
        [HttpGet("profileImage/{patientId}")]
        public async Task<IActionResult> GetProfileImage(string patientId)
        {
            var patient = await _context.Patient.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { Message = "Patient not found." });
            }

            if (patient.ProfileImage == null || patient.ProfileImage.Length == 0)
            {
                return NotFound(new { Message = "Profile image not found." });
            }

            return File(patient.ProfileImage, "image/jpeg", patient.FirstName);
        }

        
       //[HttpPost]
       // public async Task<IActionResult> AddPatient([FromForm] PatientCreateModel model)
       // {
       //     if (!ModelState.IsValid)
       //     {
       //         return BadRequest(ModelState);  // Return validation errors if any
       //     }

       //     byte[] imageData = null;

       //     // Handle Profile Image Upload (if present)
       //     if (model.ProfileImage != null && model.ProfileImage.Length > 0)
       //     {
       //         using (var ms = new MemoryStream())
       //         {
       //             await model.ProfileImage.CopyToAsync(ms);
       //             imageData = ms.ToArray();
       //         }
       //     }

       //     var patient = new Patient
       //     {
       //         PatientId = model.PatientId,
       //         FirstName = model.FirstName,
       //         LastName = model.LastName,
       //         DOB = model.DOB,
       //         Email = model.Email,
       //         BloodGroup = model.BloodGroup,
       //         NIC = model.NIC,
       //         ProfileImage = imageData,  // Save image as byte[]
       //         PasswordHash = model.PasswordHash,
       //         ContactNo = model.ContactNo,
       //         Status = "Active",  // Default status
       //         CreatedAt = DateTime.UtcNow,
       //         UpdatedAt = DateTime.UtcNow,
       //         LastLogin = null
       //     };

       //     await _context.Patient.AddAsync(patient);
       //     await _context.SaveChangesAsync();

       //     return Ok(new { Message = "Patient added successfully!", Patient = patient });
       // }
    }
}
