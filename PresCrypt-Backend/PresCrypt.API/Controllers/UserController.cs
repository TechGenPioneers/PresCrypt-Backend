using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Linq;
using System;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.Application.Services.AuthServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Hospital = PresCrypt_Backend.PresCrypt.Core.Models.Hospital;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Web;
using static System.Net.WebRequestMethods;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserController> _logger;
        private readonly IEmailService _emailService;

        private readonly IJwtService _jwtService;




        public UserController(ApplicationDbContext applicationDbContext, IEmailService emailService, ILogger<UserController> logger, IJwtService jwtService)
        {
            _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
            _passwordHasher = new PasswordHasher<User>();
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService;
        }

        [HttpPost]
        [Route("PatientRegistration")]
        public async Task<IActionResult> Registration([FromBody] PatientRegDTO patientRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(patientRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters long, include 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character." });
            }

            if (patientRegDTO.Password != patientRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // Validate gender
            if (string.IsNullOrEmpty(patientRegDTO.Gender) ||
                (patientRegDTO.Gender != "Male" && patientRegDTO.Gender != "Female"))
            {
                return BadRequest(new { message = "Gender must be either Male or Female." });
            }

            string emailLower = patientRegDTO.Email.Trim().ToLower();

            if (_applicationDbContext.User.Any(x => x.UserName == emailLower))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            using (var transaction = _applicationDbContext.Database.BeginTransaction())
            {
                try
                {
                    string hashedPassword = _passwordHasher.HashPassword(null, patientRegDTO.Password);
                    var lastUser = _applicationDbContext.User.OrderByDescending(p => p.UserId).FirstOrDefault();
                    int newId = lastUser != null && int.TryParse(lastUser.UserId.Substring(1), out int lastId) ? lastId + 1 : 1;
                    string newUserId = $"U{newId:D3}";

                    var newUser = new User
                    {
                        UserId = newUserId,
                        UserName = emailLower,
                        PasswordHash = hashedPassword,
                        Role = "Patient",
                        Patient = new List<Patient>(),
                        Doctor = new List<Doctor>(),
                        Admin = new List<Admin>()
                    };

                    _applicationDbContext.User.Add(newUser);
                    _applicationDbContext.SaveChanges();

                    var lastPatient = _applicationDbContext.Patient.OrderByDescending(p => p.PatientId).FirstOrDefault();
                    int newIdPatient = lastPatient != null && int.TryParse(lastPatient.PatientId.Substring(1), out int lastIdPatient) ? lastIdPatient + 1 : 1;
                    string newPatientId = $"P{newIdPatient:D3}";

                    var newPatient = new Patient
                    {
                        PatientId = newPatientId,
                        FirstName = patientRegDTO.FirstName,
                        LastName = patientRegDTO.LastName,
                        Email = newUser.UserName,
                        ContactNo = patientRegDTO.ContactNumber,
                        Address = patientRegDTO.Address,
                        DOB = patientRegDTO.DOB,
                        Gender = patientRegDTO.Gender, // Added gender assignment
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = patientRegDTO.Status,
                    };

                    _applicationDbContext.Patient.Add(newPatient);
                    _applicationDbContext.SaveChanges();

                    // ✅ Send welcome email (your existing email logic)
                    string welcomeEmailBody = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Welcome to PresCrypt</title>
                    </head>
                    <body style='margin: 0; padding: 0; background-color: #f8fbff;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                            <!-- Header -->
                            <div style='background: linear-gradient(135deg, #008080 0%, #00a3a3 100%); padding: 40px 30px; text-align: center;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-weight: 600;'>
                                    Welcome to PresCrypt! 🎉
                                </h1>
                                <p style='color: #e6f7f7; margin: 10px 0 0 0; font-size: 16px; font-family: -apple-system, BlinkMacSystemFont, ""Segue UI"", Roboto, sans-serif;'>
                                    Your Digital Healthcare Journey Begins
                                </p>
                            </div>

                            <!-- Content -->
                            <div style='padding: 40px 30px;'>
                                <div style='text-align: center; margin-bottom: 30px;'>
                                    <div style='width: 80px; height: 80px; background: linear-gradient(135deg, #008080, #00a3a3); border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 32px;'>
                                        👋
                                    </div>
                                </div>
    
                                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 18px; color: #2c3e50; margin-bottom: 20px; line-height: 1.6;'>
                                    Hi <strong style='color: #008080;'>{newPatient.FirstName}</strong>,
                                </p>
    
                                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 16px; color: #5a6c7d; margin-bottom: 25px; line-height: 1.7;'>
                                    Welcome to <strong style='color: #008080;'>PresCrypt</strong> – your trusted digital healthcare platform. We're excited to have you join our community of users who prioritize secure and convenient healthcare management.
                                </p>
    
                                <!-- Patient ID Card -->
                                <div style='background: linear-gradient(135deg, #f8fbff 0%, #e8f4f8 100%); border: 2px solid #008080; border-radius: 12px; padding: 25px; margin: 30px 0; text-align: center;'>
                                    <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #5a6c7d; margin: 0 0 10px 0; text-transform: uppercase; letter-spacing: 1px; font-weight: 600;'>
                                        Your Patient ID
                                    </p>
                                    <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 20px; font-weight: bold; color: #008080; margin: 0; letter-spacing: 2px; background: #ffffff; padding: 15px; border-radius: 8px; border: 1px solid #e0e0e0;'>
                                        {newPatient.PatientId}
                                    </p>
                                    <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 12px; color: #7f8c8d; margin: 10px 0 0 0;'>
                                        💡 Save this ID for future reference
                                    </p>
                                </div>
    

    
                                <div style='background: #f8f9fa; border-left: 4px solid #008080; padding: 20px; margin: 25px 0; border-radius: 4px;'>
                                    <h3 style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; color: #2c3e50; margin: 0 0 15px 0; font-size: 18px;'>
                                        🚀 What's Next?
                                    </h3>
                                    <ul style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; color: #5a6c7d; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                        <li>Log in to your secure account</li>
                                        <li>Complete your health profile</li>
                                        <li>Book your first appointment</li>
                                        <li>Explore our digital health tools such as video chat options </li>
                                        <li>Experience our AI powered chatbot</li>
                                    </ul>
                                </div>
                            </div>

                            <!-- Support Section -->
                            <div style='background: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #6c757d; margin: 0 0 15px 0;'>
                                    Need assistance? Our support team is here to help!
                                </p>
                                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; margin: 0;'>
                                    📧 <a href='mailto:prescrypt.health@gmail.com' style='color: #008080; text-decoration: none; font-weight: 600;'>prescrypt.health@gmail.com</a>
                                    <span style='color: #dee2e6; margin: 0 10px;'>•</span>

                                </p>
                            </div>

                            <!-- Footer -->
                            <div style='background: #2c3e50; padding: 25px 30px; text-align: center;'>
                                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 12px; color: #bdc3c7; margin: 0; line-height: 1.6;'>
                                    © 2025 PresCrypt. All rights reserved.<br>
                                    This is an automated email. Please do not reply to this message.
                                </p>
                            </div>
                        </div>
                    </body>
                    </html>";

                    await _emailService.SendEmailAsync(
                        newPatient.Email,
                        "🎉 Welcome to PresCrypt – Registration Successful",
                        welcomeEmailBody
                    );

                    transaction.Commit();

                    // ✅ Generate JWT token
                    var token = _jwtService.GenerateToken(newPatientId, newPatient.Email, "Patient");

                    // ✅ Send token, role, and username to frontend
                    return Ok(new
                    {
                        token = token,
                        role = "Patient",
                        username = newPatient.Email,
                        patientId = newPatientId,
                        gender = newPatient.Gender // Include gender in response if needed
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var innerMessage = ex.InnerException?.Message ?? "No inner exception";

                    return BadRequest(new
                    {
                        message = "Registration failed",
                        error = ex.Message,
                        innerError = innerMessage
                    });
                }
            }
        }


        [HttpPost]
        [Route("DoctorRegistration")]
        public async Task<IActionResult> RegisterDoctor([FromForm] DoctorRegDTO doctorRegDTO)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }
            if (doctorRegDTO.SLMCIdImage == null)
            {
                ModelState.AddModelError("SLMCIdImage", "The SLMC ID Image is required.");
                return BadRequest(ModelState);
            }

            // Process the uploaded file
            byte[] imageBytes;
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await doctorRegDTO.SLMCIdImage.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Failed to process uploaded image",
                    error = ex.Message
                });
            }

            // Validate hospital schedules
            if (string.IsNullOrEmpty(doctorRegDTO.hospitalSchedules))
            {
                return BadRequest("Hospital schedules are required.");
            }

            List<HospitalScheduleDTO> hospitalSchedules;
            try
            {
                hospitalSchedules = JsonConvert.DeserializeObject<List<HospitalScheduleDTO>>(doctorRegDTO.hospitalSchedules);
                if (hospitalSchedules == null || !hospitalSchedules.Any())
                {
                    return BadRequest("Invalid hospital schedules data.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid hospital schedules format.");
            }

            // Validate password
            const string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(doctorRegDTO.Password, passwordPattern))
            {
                return BadRequest(new
                {
                    message = "Password must be at least 6 characters with 1 uppercase, 1 lowercase, 1 number, and 1 special character."
                });
            }

            if (doctorRegDTO.Password != doctorRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // Check email uniqueness
            string emailLower = doctorRegDTO.Email.Trim().ToLowerInvariant();
            if (await _applicationDbContext.User.AnyAsync(x => x.UserName == emailLower))
            {
                return BadRequest(new { message = "Email already exists. Try again" });
            }

            // Begin transaction
            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Generate IDs
                string userId;
                string requestId;

                try
                {
                    userId = await GenerateNewUserId();
                    requestId = await GenerateNewRequestId();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Error generating IDs",
                        phase = "ID Generation",
                        details = ex.InnerException?.Message ?? ex.Message
                    });
                }

                // Step 2: Create User
                try
                {
                    var newUser = new User
                    {
                        UserId = userId,
                        UserName = emailLower,
                        PasswordHash = _passwordHasher.HashPassword(null, doctorRegDTO.Password),
                        Role = "DoctorPending",
                        EmailVerified = false, //explicitly i have defined false stastus for  doectors under verifcation
                        Patient = new List<Patient>(),
                        Doctor = new List<Doctor>(),
                        Admin = new List<Admin>()
                    };

                    await _applicationDbContext.User.AddAsync(newUser);

                    // Save just the user to find any issues
                    await _applicationDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Error creating user account",
                        phase = "User Creation",
                        details = ex.InnerException?.Message ?? ex.Message
                    });
                }

                // Step 3: Create Doctor Request
                try
                {
                    var doctorRequest = new DoctorRequest
                    {
                        RequestId = requestId,
                        FirstName = doctorRegDTO.FirstName,
                        LastName = doctorRegDTO.LastName,
                        Gender = doctorRegDTO.Gender,
                        Email = emailLower,
                        ContactNumber = doctorRegDTO.ContactNumber,
                        Specialization = doctorRegDTO.Specialization,
                        SLMCRegId = doctorRegDTO.SLMCRegId,
                        SLMCIdImage = imageBytes,
                        NIC = doctorRegDTO.NIC,
                        Charge = doctorRegDTO.Charge,
                        RequestStatus = "Pending",
                        EmailVerified = false
                    };

                    await _applicationDbContext.DoctorRequest.AddAsync(doctorRequest);

                    // Save just the doctor request to find any issues
                    await _applicationDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Error creating doctor request",
                        phase = "Doctor Request Creation",
                        details = ex.InnerException?.Message ?? ex.Message
                    });
                }

                // Step 4: Process Hospital Schedules
                try
                {
                    await ProcessHospitalSchedules(hospitalSchedules, requestId);

                    // Save the schedules
                    await _applicationDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Error processing hospital schedules",
                        phase = "Hospital Schedule Creation",
                        details = ex.InnerException?.Message ?? ex.Message
                    });
                }

                // Step 5: Commit transaction
                try
                {
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = "Registration successful",
                        requestId = requestId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Error committing transaction",
                        phase = "Transaction Commit",
                        details = ex.InnerException?.Message ?? ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                // This is a fallback catch for any other exceptions
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Registration failed with unexpected error",
                    phase = "Unknown",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // Helper Methods
        private async Task<string> GenerateNewUserId()
        {
            var lastUser = await _applicationDbContext.User
                .OrderByDescending(p => p.UserId)
                .FirstOrDefaultAsync();

            int newId = lastUser != null && int.TryParse(lastUser.UserId.AsSpan(1), out int lastId)
                ? lastId + 1 : 1;
            return $"U{newId:D3}";
        }

        private async Task<string> GenerateNewRequestId()
        {
            var lastRequest = await _applicationDbContext.DoctorRequest
                .OrderByDescending(d => d.RequestId)
                .FirstOrDefaultAsync();

            int newId = lastRequest != null && int.TryParse(lastRequest.RequestId.AsSpan(2), out int lastReqId)
                ? lastReqId + 1 : 1;
            return $"R{newId:D3}";
        }

        private async Task ProcessHospitalSchedules(List<HospitalScheduleDTO> schedules, string doctorRequestId)
        {
            if (schedules?.Any() != true) return;

            // 1. Get the current maximum ID from the database (e.g., "AR10")
            var maxId = await _applicationDbContext.RequestAvailability
                .Select(ra => ra.AvailabilityRequestId)
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync();

            // 2. Extract the numeric part (if no records, start at 1)
            int nextId = 1; // Default if table is empty
            if (!string.IsNullOrEmpty(maxId))
            {
                var numericPart = maxId.Substring(2); // Skip "AR"
                if (int.TryParse(numericPart, out int lastId))
                {
                    nextId = lastId + 1; // Increment
                }
            }

            var availabilityRequests = new List<RequestAvailability>();

            foreach (var schedule in schedules)
            {
                if (schedule.availability == null) continue;

                bool hospitalExists = await _applicationDbContext.Hospitals
                    .AnyAsync(h => h.HospitalId == schedule.hospitalId);

                if (!hospitalExists) continue;

                foreach (var (day, time) in schedule.availability)
                {
                    if (time == null) continue;

                    if (TimeOnly.TryParse(time.startTime, out var startTime) &&
                        TimeOnly.TryParse(time.endTime, out var endTime))
                    {
                        availabilityRequests.Add(new RequestAvailability
                        {
                            AvailabilityRequestId = $"AR{nextId++.ToString("D3")}",
                            DoctorRequestId = doctorRequestId,
                            AvailableDay = day,
                            AvailableStartTime = startTime,
                            AvailableEndTime = endTime,
                            HospitalId = schedule.hospitalId
                        });
                    }
                }
            }

            if (availabilityRequests.Any())
            {
                await _applicationDbContext.RequestAvailability.AddRangeAsync(availabilityRequests);
            }
        }

        [HttpPost]
        [Route("AdminRegistration")]
        public IActionResult RegisterAdmin([FromBody] AdminRegDTO adminRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            // Validate Password
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(adminRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters long, include 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character." });
            }

            if (adminRegDTO.Password != adminRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // Normalize email
            string emailLower = adminRegDTO.Email.Trim().ToLower();

            // Check if the email already exists in the User table
            if (_applicationDbContext.User.Any(x => x.UserName == emailLower))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            using (var transaction = _applicationDbContext.Database.BeginTransaction())
            {
                try
                {
                    string hashedPassword = _passwordHasher.HashPassword(null, adminRegDTO.Password);

                    // Generate UserId for User table (e.g., U001, U002, etc.)
                    var lastUser = _applicationDbContext.User.OrderByDescending(a => a.UserId).FirstOrDefault();
                    int newUserId = lastUser != null && int.TryParse(lastUser.UserId.Substring(1), out int lastUserNumber) ? lastUserNumber + 1 : 1;
                    string newUserIdPrefix = $"U{newUserId:D3}"; // UserId in User table will be like U001, U002, ...

                    // Step 1: Insert into User table first
                    var newUser = new User
                    {
                        UserId = newUserIdPrefix,  // Correct UserId prefix (U)
                        UserName = emailLower,
                        PasswordHash = hashedPassword,
                        Role = "Admin",  // Default role is "Admin"
                        Patient = new List<Patient>(),
                        Doctor = new List<Doctor>(),
                        Admin = new List<Admin>() // Associate Admin role
                    };

                    _applicationDbContext.User.Add(newUser);
                    _applicationDbContext.SaveChanges();

                    // Generate AdminId for Admin table (e.g., A001, A002, etc.)
                    var lastAdmin = _applicationDbContext.Admin.OrderByDescending(a => a.AdminId).FirstOrDefault();
                    int newAdminId = lastAdmin != null && int.TryParse(lastAdmin.AdminId.Substring(1), out int lastAdminNumber) ? lastAdminNumber + 1 : 1;
                    string newAdminIdPrefix = $"A{newAdminId:D3}"; // AdminId in Admin table will be like A001, A002, ...

                    // Step 2: Insert into Admin table
                    var newAdmin = new Admin
                    {
                        AdminId = newAdminIdPrefix,  // Correct AdminId prefix (A)
                        FirstName = adminRegDTO.FirstName,
                        LastName = adminRegDTO.LastName,
                        Email = emailLower,
                        Role = "Admin",  // Admin role
                        PasswordHash = hashedPassword,

                        LastLogin = DateTime.UtcNow  // Set last login time
                    };

                    _applicationDbContext.Admin.Add(newAdmin);
                    _applicationDbContext.SaveChanges();

                    transaction.Commit(); // Commit both inserts

                    return Ok(new { message = "Admin registered successfully", adminId = newAdmin.AdminId });
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    // Log or return the detailed exception information
                    return BadRequest(new { message = "Doctor registration failed", error = ex.InnerException?.Message ?? ex.Message });
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback if any issue occurs
                    return BadRequest(new { message = "Registration failed", error = ex.Message });
                }
            }
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                string inputEmail = loginDTO.Email.Trim().ToLower();

                var user = _applicationDbContext.User.FirstOrDefault(u => u.UserName.ToLower() == inputEmail);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Invalid email or password." });
                }

                // ✅ FIXED: Auto-unlock logic - Check FIRST before manual block check
                if (user.IsBlocked && user.LastFailedLoginTime.HasValue)
                {
                    // Check if 15 minutes have passed since last failed attempt
                    if (user.LastFailedLoginTime.Value.AddMinutes(15) <= DateTime.UtcNow)
                    {
                        // Auto-unlock the account
                        user.IsBlocked = false;
                        user.FailedLoginAttempts = 0;
                        user.LastFailedLoginTime = null;
                        await _applicationDbContext.SaveChangesAsync();

                        _logger.LogInformation($"🔓 Account auto-unlocked for {user.UserName} after 15 minutes");

                        // Send auto-unlock notification email
                        string unlockEmailBody = $@"
                            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                                <p>Dear user,</p>
                                <p><strong>Good News!</strong> We have observed that you have logged in successfully again once after your account has been now unlocked.</p>
                    
                                <p>For your security, please ensure you're using the correct password.</p>
                                <br/>
                                <p>Best regards,<br/>PresCrypt Security Team</p>
                            </div>";
                        await _emailService.SendEmailAsync(user.UserName, "✅ Account Unlocked - PresCrypt", unlockEmailBody);
                    }
                    else
                    {
                        // Still within 15-minute lockout period
                        var remainingTime = user.LastFailedLoginTime.Value.AddMinutes(15) - DateTime.UtcNow;
                        int remainingMinutes = (int)Math.Ceiling(remainingTime.TotalMinutes);

                        return BadRequest(new
                        {
                            success = false,
                            message = $"🚫 Account is temporarily locked. Please try again in {remainingMinutes} minute(s).",
                            remainingMinutes = remainingMinutes
                        });
                    }
                }

                // ✅ Check if account is still manually blocked (this should rarely happen now)
                if (user.IsBlocked)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "🚫 Your account is blocked. Please contact admin to unlock it."
                    });
                }

                if (user.Role == "DoctorPending")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Your doctor account is pending approval. Please wait for confirmation."
                    });
                }
                else if (!user.EmailVerified)
                {
                    return BadRequest(new { success = false, message = "Please verify your email before logging in." });
                }

                var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDTO.Password);

                if (result != PasswordVerificationResult.Success)
                {
                    user.FailedLoginAttempts += 1;
                    user.LastFailedLoginTime = DateTime.UtcNow;

                    // ✅ Block account after 5 failed attempts
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsBlocked = true;

                        // Send account blocked email with enhanced template
                        string blockEmailBody = $@"
                            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px;'>
                                <div style='background: #f8d7da; border: 1px solid #f5c6cb; border-radius: 5px; padding: 15px; margin-bottom: 20px;'>
                                    <h3 style='color: #721c24; margin: 0 0 10px 0;'>🚫 Account Temporarily Locked</h3>
                                    <p style='color: #721c24; margin: 0;'><strong>Security Notice:</strong> Your account has been locked after 5 failed login attempts.</p>
                                </div>
                    
                                <p>Dear user,</p>
                                <p>For your account security, we have temporarily locked your account due to multiple unsuccessful login attempts.</p>
                    
                                <div style='background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 5px; padding: 15px; margin: 15px 0;'>
                                    <h4 style='color: #0c5460; margin: 0 0 10px 0;'>⏰ Auto-Unlock Information:</h4>
                                    <p style='color: #0c5460; margin: 0;'>Your account will be automatically unlocked after <strong>15 minutes</strong> from the last failed attempt.</p>
                                    <p style='color: #0c5460; margin: 5px 0 0 0; font-size: 12px;'>Locked at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                                </div>
                    
                                <p><strong>What you can do:</strong></p>
                                <ul>
                                    <li>Wait 15 minutes and try logging in again</li>
                                    <li>Make sure you're using the correct password</li>
                                    <li>Use the 'Forgot Password' feature if needed</li>
                                    <li>Contact support if you continue having issues</li>
                                </ul>
                    
                                <hr style='margin: 20px 0; border: none; border-top: 1px solid #e0e0e0;' />
                                <p style='font-size: 12px; color: #6c757d;'>
                                    Best regards,<br/>
                                    PresCrypt Security Team<br/>
                                    📧 prescrypt.health@gmail.com
                                </p>
                            </div>";

                        await _emailService.SendEmailAsync(user.UserName, "🚫 Account Temporarily Locked - PresCrypt", blockEmailBody);

                        _logger.LogWarning($"🚫 Account locked for {user.UserName} after 5 failed attempts");
                    }
                    else if (user.FailedLoginAttempts == 4)
                    {
                        // Use your enhanced security alert template here
                        string emailBody = @"
                            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                                <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin-bottom: 15px;'>
                                    <p style='color: #856404; margin: 0;'><strong>⚠️ Security Alert:</strong> You have entered an incorrect password <strong>4 times</strong>.</p>
                                </div>
                                <p>Dear user,</p>
                                <p>We detected multiple failed login attempts on your account.</p>
                                <p><strong style='color: #dc3545;'>Warning:</strong> If you enter the wrong password one more time, your account will be <strong>temporarily locked for 15 minutes</strong>.</p>
                    
                                <div style='background: #e8f4f8; border-left: 4px solid #008080; padding: 15px; margin: 15px 0;'>
                                    <p style='margin: 0;'><strong>💡 Helpful Tips:</strong></p>
                                    <ul style='margin: 10px 0 0 0;'>
                                        <li>Double-check your password for typos</li>
                                        <li>Ensure Caps Lock is not on</li>
                                        <li>Use 'Forgot Password' if you're unsure</li>
                                    </ul>
                                </div>
                    
                                <br/>
                                <p>Best regards,<br/>PresCrypt Security Team</p>
                            </div>";
                        await _emailService.SendEmailAsync(user.UserName, "⚠️ Security Alert: Final Warning - PresCrypt", emailBody);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                    return BadRequest(new { success = false, message = "Invalid email or password." });
                }

                // ✅ Reset on successful login
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginTime = null;
                user.IsBlocked = false; // Ensure it's reset

                // ✅ Set LastLogin in respective table
                if (user.Role == "Admin")
                {
                    var admin = await _applicationDbContext.Admin.FirstOrDefaultAsync(a => a.Email == user.UserName);
                    if (admin != null)
                    {
                        admin.LastLogin = DateTime.UtcNow;
                    }
                }
                else if (user.Role == "Doctor")
                {
                    var doctor = await _applicationDbContext.Doctor.FirstOrDefaultAsync(d => d.Email == user.UserName);
                    if (doctor != null)
                    {
                        doctor.LastLogin = DateTime.UtcNow;
                    }
                }
                else if (user.Role == "Patient")
                {
                    var patient = await _applicationDbContext.Patient.FirstOrDefaultAsync(p => p.Email == user.UserName);
                    if (patient != null)
                    {
                        patient.LastLogin = DateTime.UtcNow;
                    }
                }

                // Admin 2FA
                if (user.Role == "Admin")
                {
                    string code = new Random().Next(100000, 999999).ToString();
                    user.TwoFactorCode = code;
                    user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
                    await _applicationDbContext.SaveChangesAsync();

                    string verifyUrl = $"http://localhost:3000/Auth/Verify2FA?email={Uri.EscapeDataString(user.UserName)}";

                    // Use your enhanced 2FA template here
                    string twoFAEmailBody = $@"
                        <!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Your 2FA Verification Code - PresCrypt</title>
                        </head>
                        <body style='margin: 0; padding: 0; background-color: #f8fbff;'>
                            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                <!-- Header -->
                                <div style='background: linear-gradient(135deg, #008080 0%, #00a3a3 100%); padding: 30px; text-align: center;'>
                                    <div style='background: rgba(255,255,255,0.2); border-radius: 50%; width: 80px; height: 80px; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 32px;'>
                                        🔐
                                    </div>
                                    <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-weight: 600;'>
                                        Two-Factor Authentication
                                    </h1>
                                </div>
        
                                <!-- Content -->
                                <div style='padding: 40px 30px; text-align: center;'>
                                    <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 18px; color: #2c3e50; margin-bottom: 25px; line-height: 1.6;'>
                                        Your verification code is ready! 🎯
                                    </p>
            
                                    <!-- Code Box -->
                                    <div style='background: linear-gradient(135deg, #f8fbff 0%, #e8f4f8 100%); border: 3px solid #008080; border-radius: 15px; padding: 30px; margin: 30px 0; display: inline-block;'>
                                        <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #5a6c7d; margin: 0 0 15px 0; text-transform: uppercase; letter-spacing: 1px; font-weight: 600;'>
                                            Your 2FA Code
                                        </p>
                                        <p style='font-family: Monaco, Consolas, monospace; font-size: 36px; font-weight: bold; color: #008080; margin: 0; letter-spacing: 8px; background: #ffffff; padding: 20px 30px; border-radius: 10px; border: 2px solid #e0e0e0; text-align: center; min-width: 200px;'>
                                            {code}
                                        </p>
                                    </div>
            
                                    <!-- Timer Alert -->
                                    <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; margin: 25px 0; text-align: center;'>
                                        <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 16px; color: #856404; margin: 0; font-weight: 600;'>
                                            ⏰ <strong>Expires in 5 minutes</strong>
                                        </p>
                                        <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #856404; margin: 5px 0 0 0;'>
                                            Enter this code quickly to complete your login
                                        </p>
                                    </div>
            
                                    <!-- Quick Action Button -->
                                    <div style='margin: 35px 0;'>
                                        <a href='{verifyUrl}' style='display: inline-block; background: linear-gradient(135deg, #008080 0%, #00a3a3 100%); color: #ffffff; padding: 15px 35px; text-decoration: none; border-radius: 25px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-weight: 600; font-size: 16px; box-shadow: 0 4px 12px rgba(0,128,128,0.3);'>
                                            🚀 Verify Now
                                        </a>
                                    </div>
            
                                    <!-- Instructions -->
                                    <div style='background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 25px 0; text-align: left;'>
                                        <h3 style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; color: #2c3e50; margin: 0 0 15px 0; font-size: 16px; text-align: center;'>
                                            📱 How to Use This Code
                                        </h3>
                                        <ol style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; color: #5a6c7d; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                            <li>Return to the PresCrypt login page</li>
                                            <li>Enter the 6-digit code above</li>
                                            <li>Click ""Verify"" to complete your login</li>
                                        </ol>
                                    </div>
                                </div>
        
                                <!-- Footer -->
                                <div style='background: #2c3e50; padding: 25px 30px; text-align: center;'>
                                    <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 12px; color: #bdc3c7; margin: 0;'>
                                        © 2025 PresCrypt Security • This code was generated for your account security
                                    </p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(user.UserName, "🔐 Your Admin 2FA Code - PresCrypt", twoFAEmailBody);

                    return Ok(new
                    {
                        success = true,
                        message = "2FA code sent to your email.",
                        twoFactorRequired = true,
                        email = user.UserName
                    });
                }

                // Generate JWT
                var token = _jwtService.GenerateToken(user.UserId, user.UserName, user.Role);

                await _applicationDbContext.SaveChangesAsync();
                _logger.LogInformation($"✅ Login successful for {user.UserName}");

                return Ok(new
                {
                    success = true,
                    message = $"{user.Role} login successful",
                    token = token,
                    user = new
                    {
                        id = user.UserId,
                        username = user.UserName,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}", ex);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        [HttpPost]
        [Route("Verify2FA")]
        public IActionResult Verify2FA([FromBody] TwoFactorDTO model)
        {
            var user = _applicationDbContext.User.FirstOrDefault(u => u.UserName.ToLower() == model.Email.ToLower());

            if (user == null || user.TwoFactorCode != model.Code || user.TwoFactorExpiry < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired 2FA code." });
            }

            // Clear the code after successful use
            user.TwoFactorCode = null;
            user.TwoFactorExpiry = null;

            var token = _jwtService.GenerateToken(user.UserId, user.UserName, user.Role);


            _applicationDbContext.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "2FA verified successfully",
                token = token,
                user = new
                {
                    id = user.UserId,
                    username = user.UserName,
                    role = user.Role
                }
            });
        }



        /// Test route to check if the user is authenticated
        [Authorize(Roles = "Patient")]
        [HttpGet("test-protected")]
        public IActionResult TestProtected()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Access granted to protected route",
                userId,
                email,
                role
            });
        }



        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetUsers()
        {
            return Ok(_applicationDbContext.User.ToList());
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("GetAllPatients")]
        public IActionResult GetPatients()
        {
            return Ok(_applicationDbContext.Patient.ToList());
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("GetAllDoctors")]
        public IActionResult GetDoctors()
        {
            return Ok(_applicationDbContext.Doctor.ToList());
        }
        [HttpGet]
        [Route("GetAllAdmins")]
        public IActionResult GetAdmins()
        {
            return Ok(_applicationDbContext.Admin.ToList());
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Normalize and find user
            var normalizedEmail = model.Email.Trim().ToLower();
            var user = await _applicationDbContext.User
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedEmail);

            // Security: Always return success to prevent email enumeration
            if (user == null)
                return Ok(new { message = "If this email exists, a reset link was sent." });

            // Generate cryptographically secure token
            user.ResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace('+', '-').Replace('/', '_').Replace("=", "");
            user.ResetTokenExpire = DateTime.UtcNow.AddHours(1);

            // Create secure reset link
            string resetLink = $"http://localhost:3000/Auth/ResetPassword?" +
                $"token={HttpUtility.UrlEncode(user.ResetToken)}&" +
                $"email={HttpUtility.UrlEncode(user.UserName)}";

            // Send email
            await SendResetEmailAsync(user.UserName, resetLink);

            await _applicationDbContext.SaveChangesAsync();
            return Ok(new { message = "If this email exists, a reset link was sent." });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(model.Token))
                return BadRequest("Token is required");

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest("NewPassword is required");

            // Find user
            var normalizedEmail = model.Email.Trim().ToLower();
            var user = await _applicationDbContext.User
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedEmail);

            // Validate token
            if (user == null || user.ResetToken != model.Token)
                return BadRequest(new { message = "Invalid token." });

            if (user.ResetTokenExpire < DateTime.UtcNow)
                return BadRequest(new { message = "Token expired." });

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpire = null;

            await _applicationDbContext.SaveChangesAsync();
            return Ok(new { message = "Password reset successful." });
        }

        private async Task SendResetEmailAsync(string email, string resetLink)
        {
            string passwordResetEmailBody = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password - PresCrypt</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fbff;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #008080 0%, #00a3a3 100%); padding: 30px; text-align: center;'>
            <div style='background: rgba(255,255,255,0.2); border-radius: 50%; width: 80px; height: 80px; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 32px;'>
                🔐
            </div>
            <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-weight: 600;'>
                Password Reset Request
            </h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 40px 30px;'>
            <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 18px; color: #2c3e50; margin-bottom: 20px; line-height: 1.6;'>
                Hello there! 👋
            </p>
            
            <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 16px; color: #5a6c7d; margin-bottom: 25px; line-height: 1.7;'>
                We received a request to reset your PresCrypt account password. No worries – it happens to the best of us!
            </p>
            
            <!-- Alert Box -->
            <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; margin: 25px 0;'>
                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #856404; margin: 0; font-weight: 600;'>
                    ⏰ <strong>Time Sensitive:</strong> This reset link expires in 1 hour for your security.
                </p>
            </div>
            
            <!-- CTA Button -->
            <div style='text-align: center; margin: 35px 0;'>
                <a href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #008080 0%, #00a3a3 100%); color: #ffffff; padding: 18px 40px; text-decoration: none; border-radius: 25px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-weight: 600; font-size: 18px; box-shadow: 0 4px 12px rgba(0,128,128,0.3); transition: all 0.3s ease;'>
                    🔑 Reset My Password
                </a>
            </div>        
            <!-- Security Notice -->
            <div style='background: #e8f4f8; border: 1px solid #008080; border-radius: 8px; padding: 20px; margin: 30px 0;'>
                <h3 style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; color: #008080; margin: 0 0 10px 0; font-size: 16px;'>
                    🛡️ Security Notice
                </h3>
                <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #2c3e50; margin: 0; line-height: 1.6;'>
                    If you didn't request this password reset, please ignore this email. Your account remains secure, and no action is needed.
                </p>
            </div>
        </div>
        
        <!-- Footer -->
        <div style='background: #f8f9fa; padding: 25px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
            <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 14px; color: #6c757d; margin: 0 0 10px 0;'>
                Best regards,<br><strong style='color: #008080;'>The PresCrypt Security Team</strong>
            </p>
        </div>
        
        <div style='background: #2c3e50; padding: 20px 30px; text-align: center;'>
            <p style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; font-size: 12px; color: #bdc3c7; margin: 0;'>
                © 2025 PresCrypt. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";

            await _emailService.SendEmailAsync(
                email,
                "Reset Your Password",
                passwordResetEmailBody
            );
        }
        [HttpGet]
        [Route("GetUserRole")]
        [Authorize]
        public IActionResult GetUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Role not found." });
            }
            return Ok(new { role });
        }


        [HttpGet]
        [Route("GetAllHospitals")]
        public IActionResult GetHospitals()
        {
            return Ok(_applicationDbContext.Hospitals.ToList());
        }

        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }

    }
}