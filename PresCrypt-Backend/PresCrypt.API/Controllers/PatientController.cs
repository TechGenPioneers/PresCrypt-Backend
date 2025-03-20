using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ApplicationDbContext applicationDbContext;
        public PatientController(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;

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


    }
}