using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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


[HttpPost]
    [Route("Registration")]
    public IActionResult Registration(PatientRegDTO patientRegDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data", errors = ModelState });
        }
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";

            if (string.IsNullOrEmpty(patientRegDTO.Password) || !Regex.IsMatch(patientRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { error = "Password must be at least 6 characters long, include 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character." });
            }

            //  Check if passwords match
            if (patientRegDTO.Password != patientRegDTO.ConfirmPassword)
            {
                return BadRequest(new { error = "Passwords do not match." });
            }
            // Normalize email to ensure case-insensitive uniqueness
            string emailLower = patientRegDTO.Email.ToLower();

        // Check if the email already exists
        var existingUser = applicationDbContext.Patient.FirstOrDefault(x => x.Email.ToLower() == emailLower);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email already exists" });
        }

        // Get the latest Patient ID
        var lastPatient = applicationDbContext.Patient
            .OrderByDescending(p => p.PatientId)
            .FirstOrDefault();

        int newId = 1; // Default first ID
        if (lastPatient != null)
        {
            string lastId = lastPatient.PatientId.Replace("P", ""); // Remove "P"
            if (int.TryParse(lastId, out int lastNum))
            {
                newId = lastNum + 1; // Increment last number
            }
        }

        string newPatientId = $"P{newId:D3}";

        // Hash the password before saving
        string hashedPassword = HashPassword(patientRegDTO.Password);

        // Create new patient record
        var newPatient = new Patient
        {
            PatientId = newPatientId,
            PatientName = patientRegDTO.FullName,
            Email = emailLower,
            PasswordHash = hashedPassword,
            ContactNo = patientRegDTO.ContactNumber,
            NIC = patientRegDTO.NIC,
            BloodGroup = patientRegDTO.BloodGroup,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = patientRegDTO.Status
        };

        applicationDbContext.Patient.Add(newPatient);
        applicationDbContext.SaveChanges();

        return Ok(new { message = "Patient registered successfully", patientId = newPatientId });
    }

    // Password hashing function
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }



    [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDTO patientLoginDTO)
        {
            if (patientLoginDTO == null || string.IsNullOrEmpty(patientLoginDTO.Email) || string.IsNullOrEmpty(patientLoginDTO.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            Patient? objUser = applicationDbContext.Patient.FirstOrDefault(x => x.Email == patientLoginDTO.Email);

            if (objUser == null)
            {
                return BadRequest(new { message = "Invalid Email." });
            }

            if (objUser.PasswordHash != patientLoginDTO.Password)
            {
                return BadRequest(new { message = "Invalid Password." });
            }

            return Ok(new
            {
                message = "Login Successful",
                user = new
                {
                    id = objUser.PatientId,
                    email = objUser.Email,
                    name = objUser.PatientName
                }
            });
        }




        [HttpGet]
        [Route("GetPatientById/{id}")]
        public IActionResult GetPatientById(string id)
        {
            var patient = applicationDbContext.Patient.FirstOrDefault(x => x.PatientId == id);
            if (patient != null)
            {
                return Ok(patient);
            }
            else
            {
                return BadRequest("Patient Not Found");
            }

        }

        [HttpGet]
        [Route("GetPatients")]
        public IActionResult GetPatients()
        {
            return Ok(applicationDbContext.Patient.ToList());
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

        
  
    }
}
