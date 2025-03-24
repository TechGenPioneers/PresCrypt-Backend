using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Application.Services.AuthServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly PasswordHasher<Patient> _passwordHasher;
        private readonly IEmailService _emailService;

        public PatientController(ApplicationDbContext applicationDbContext, IEmailService emailService)
        {
            _applicationDbContext = applicationDbContext;
            _passwordHasher = new PasswordHasher<Patient>();
            _emailService = emailService;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration([FromBody] PatientRegDTO patientRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

          

            // Validate Password
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(patientRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters long, include 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character." });
            }

            if (patientRegDTO.Password != patientRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // Normalize email
            string emailLower = patientRegDTO.Email.Trim().ToLower();
            if (_applicationDbContext.Patient.Any(x => x.Email == emailLower))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            // Generate PatientId
            var lastPatient = _applicationDbContext.Patient.OrderByDescending(p => p.PatientId).FirstOrDefault();
            int newId = lastPatient != null && int.TryParse(lastPatient.PatientId.Substring(1), out int lastId) ? lastId + 1 : 1;
            string newPatientId = $"P{newId:D3}";

            var newPatient = new Patient
            {
                PatientId = newPatientId,
                PatientName = patientRegDTO.FullName,
                Email = emailLower,
                ContactNo = patientRegDTO.ContactNumber,
                NIC = patientRegDTO.NIC,
               // BloodGroup = patientRegDTO.BloodGroup,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = patientRegDTO.Status
            };

            // Hash password
            newPatient.PasswordHash = _passwordHasher.HashPassword(newPatient, patientRegDTO.Password);

            _applicationDbContext.Patient.Add(newPatient);
            _applicationDbContext.SaveChanges();

            return Ok(new { message = "Patient registered successfully", patientId = newPatientId });
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDTO patientLoginDTO)
        {
            if (string.IsNullOrEmpty(patientLoginDTO.Email) || string.IsNullOrEmpty(patientLoginDTO.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            var user = _applicationDbContext.Patient.FirstOrDefault(x => x.Email.ToLower() == patientLoginDTO.Email.ToLower());
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, patientLoginDTO.Password);
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

        [HttpGet]
        [Route("GetPatients")]
        public IActionResult GetPatients()
        {
            return Ok(_applicationDbContext.Patient.ToList());
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var user = _applicationDbContext.Patient.FirstOrDefault(x => x.Email.ToLower() == model.Email.ToLower());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Generate a URL-safe token
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            // Save token in DB
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _applicationDbContext.SaveChanges();

            // Send email with reset link
            string resetLink = $"https://yourfrontend.com/reset-password?token={token}&email={model.Email}";

            await _emailService.SendEmailAsync(user.Email, "Reset Password",
                $"Click the link to reset your password: {resetLink}");

            return Ok(new { message = "Password reset link sent to your email." });
        }
    }
}
