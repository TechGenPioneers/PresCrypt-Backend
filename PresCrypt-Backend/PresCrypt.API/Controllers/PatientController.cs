using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;

        private readonly PasswordHasher<Patient> _passwordHasher;

        public PatientController(ApplicationDbContext context)
        {
            _applicationDbContext = applicationDbContext;
            _passwordHasher = new PasswordHasher<Patient>(); // ✅ Properly initialize password hasher
        }

        // ✅ PATIENT REGISTRATION
        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration([FromBody] PatientRegDTO patientRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            if (string.IsNullOrEmpty(patientRegDTO.BloodGroup))
            {
                return BadRequest(new { message = "Blood Group is required." });
            }

            // ✅ Password validation
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (string.IsNullOrEmpty(patientRegDTO.Password) || !Regex.IsMatch(patientRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters long, include 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character." });
            }

            if (patientRegDTO.Password != patientRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // ✅ Convert email to lowercase & check if it already exists
            string emailLower = patientRegDTO.Email.ToLower();
            if (_applicationDbContext.Patient.Any(x => x.Email == emailLower))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            // ✅ Generate new Patient ID
            var lastPatient = _applicationDbContext.Patient.OrderByDescending(p => p.PatientId).FirstOrDefault();
            int newId = (lastPatient != null) ? int.Parse(lastPatient.PatientId.Replace("P", "")) + 1 : 1;
            string newPatientId = $"P{newId:D3}";

            // ✅ Hash the password securely
            string hashedPassword = _passwordHasher.HashPassword(null, patientRegDTO.Password);

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

            _applicationDbContext.Patient.Add(newPatient);
            _applicationDbContext.SaveChanges();

            return Ok(new { message = "Patient registered successfully", patientId = newPatientId });
        }

        // ✅ PATIENT LOGIN
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDTO patientLoginDTO)
        {
            if (patientLoginDTO == null || string.IsNullOrEmpty(patientLoginDTO.Email) || string.IsNullOrEmpty(patientLoginDTO.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            // ✅ Find the user by email
            var user = _applicationDbContext.Patient.FirstOrDefault(x => x.Email.ToLower() == patientLoginDTO.Email.ToLower());
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            // ✅ Verify the hashed password
            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, patientLoginDTO.Password);
            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                success = true,
                message = "Login Successful",
                user = new
                {
                    id = user.PatientId,
                    email = user.Email,
                    name = user.PatientName
                }
            });
        }

        // ✅ GET PATIENT BY ID
        [HttpGet]
        [Route("GetPatientById/{id}")]
        public IActionResult GetPatientById(string id)
        {
            var patient = _applicationDbContext.Patient.FirstOrDefault(x => x.PatientId == id);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }
            return Ok(patient);
        }

        // ✅ GET ALL PATIENTS
        [HttpGet]
        [Route("GetPatients")]
        public IActionResult GetPatients()
        {
            var patients = _applicationDbContext.Patient.ToList();
            return Ok(patients);
        }
    }
}
