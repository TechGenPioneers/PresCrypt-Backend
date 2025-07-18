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
        public IActionResult Registration([FromBody] PatientRegDTO patientRegDTO)
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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = patientRegDTO.Status,

                    };

                    _applicationDbContext.Patient.Add(newPatient);
                    _applicationDbContext.SaveChanges();

                    transaction.Commit();

                    return Ok(new { message = "Patient registered successfully", patientId = newPatientId });
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


        //[HttpPost]
        //[Route("Login")]
        //public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
        //        {
        //            return BadRequest(new { message = "Email and password are required." });
        //        }

        //        string inputEmail = loginDTO.Email.Trim().ToLower();

        //        var user = _applicationDbContext.User.FirstOrDefault(u => u.UserName.ToLower() == inputEmail);

        //        if (user == null)
        //        {
        //            return BadRequest(new { success = false, message = "Invalid email or password." });
        //        }

        //        if (user.Role == "DoctorPending")
        //        {
        //            return BadRequest(new
        //            {
        //                success = false,
        //                message = "Your doctor account is pending approval. Please wait for confirmation."
        //            });
        //        }
        //        else if (!user.EmailVerified)
        //        {
        //            return BadRequest(new { success = false, message = "Please verify your email before logging in." });
        //        }

        //        // Check for lockout
        //        // Check and reset if 15 minutes have passed since last failed attempt
        //        if (user.FailedLoginAttempts >= 5 && user.LastFailedLoginTime.HasValue)
        //        {
        //            if (user.LastFailedLoginTime.Value.AddMinutes(15) <= DateTime.UtcNow)
        //            {
        //                // Reset the lockout
        //                user.FailedLoginAttempts = 0;
        //                user.LastFailedLoginTime = null;
        //                await _applicationDbContext.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                return BadRequest(new { message = "Account locked due to too many failed attempts. Try again later." });
        //            }
        //        }


        //        var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDTO.Password);

        //        if (result != PasswordVerificationResult.Success)
        //        {
        //            user.FailedLoginAttempts += 1;
        //            user.LastFailedLoginTime = DateTime.UtcNow;
        //            if (user.FailedLoginAttempts == 4)
        //            {
        //                string emailBody = @"
        //<div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
        //    <p>Dear user,</p>

        //    <p><strong>Security Alert:</strong> You have entered an incorrect password <strong>4 times</strong>.</p>

        //    <p>If you enter the wrong password one more time, your account will be <strong>temporarily locked</strong> for <strong>15 minutes</strong>.</p>

        //    <p>If this wasn't you, we recommend changing your password immediately or contacting support.</p>

        //    <br/>
        //    <p>Best regards,<br/>Security Team - PresCrypt</p>
        //</div>";

        //                await _emailService.SendEmailAsync(user.UserName, "⚠️ Security Alert: Failed Login Attempts", emailBody);
        //            }


        //            await _applicationDbContext.SaveChangesAsync();

        //            return BadRequest(new { success = false, message = "Invalid email or password." });
        //        }

        //        // Reset failed attempts after successful login
        //        user.FailedLoginAttempts = 0;
        //        user.LastFailedLoginTime = null;

        //        // 🔐 ADMIN ONLY 2FA
        //        if (user.Role == "Admin")
        //        {
        //            string code = new Random().Next(100000, 999999).ToString();
        //            user.TwoFactorCode = code;
        //            user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
        //            await _applicationDbContext.SaveChangesAsync();

        //            string verifyUrl = $"http://localhost:3000/Auth/Verify2FA?email={Uri.EscapeDataString(user.UserName)}";
        //            string emailBody = $@"
        //                <p>Your 2FA code is: <strong>{code}</strong></p>
        //                <p>This code will expire in 5 minutes.</p>
        //                <p>Please <a href='{verifyUrl}'>click here to verify your 2FA code</a> or copy and paste this link into your browser:</p>

        //                <br/>
        //                <p>If you did not request this login, please ignore this email.</p>";


        //            await _emailService.SendEmailAsync(user.UserName, "Your Admin 2FA Code", emailBody);

        //            return Ok(new
        //            {
        //                success = true,
        //                message = "2FA code sent to your email.",
        //                twoFactorRequired = true,
        //                email = user.UserName
        //            });
        //        }

        //        // For other roles, return token directly
        //        var token = _jwtService.GenerateToken(user.UserId, user.UserName, user.Role);
        //        Response.Cookies.Append("authToken", token, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = false, // Set to true in production with HTTPS
        //            SameSite = SameSiteMode.Strict,
        //            Expires = DateTimeOffset.UtcNow.AddHours(1)
        //        });

        //        await _applicationDbContext.SaveChangesAsync();
        //        _logger.LogInformation($"Successful login for {user.UserName}");

        //        return Ok(new
        //        {
        //            success = true,
        //            message = $"{user.Role} login successful",
        //            token = token,
        //            user = new
        //            {
        //                id = user.UserId,
        //                username = user.UserName,
        //                role = user.Role
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Login error: {ex.Message}", ex);
        //        return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        //    }
        //}

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

                // ✅ NEW: Check if account is manually blocked
                if (user.IsBlocked)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "🚫 Your account is blocked due to multiple failed attempts. Please contact admin to unlock it."
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

                // ✅ Auto-reset if 15 min passed, but only if not blocked
                if (user.FailedLoginAttempts >= 5 && user.LastFailedLoginTime.HasValue)
                {
                    if (user.LastFailedLoginTime.Value.AddMinutes(15) <= DateTime.UtcNow)
                    {
                        user.FailedLoginAttempts = 0;
                        user.LastFailedLoginTime = null;
                        // ⚠️ Do NOT reset IsBlocked here — this must be done manually by admin
                        await _applicationDbContext.SaveChangesAsync();
                    }
                    else
                    {
                        return BadRequest(new { message = "Account temporarily locked due to multiple failed login attempts. Try again later." });
                    }
                }

                var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDTO.Password);

                if (result != PasswordVerificationResult.Success)
                {
                    user.FailedLoginAttempts += 1;
                    user.LastFailedLoginTime = DateTime.UtcNow;

                    // ✅ NEW: Lock permanently if 5+ attempts
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsBlocked = true;

                        // Optional: Notify admin or user
                        string blockEmail = @"
                    <p><strong>Security Notice:</strong><br/>
                    Your account has been <strong>blocked</strong> after 5 failed login attempts.<br/>
                    Please contact the system administrator to reactivate your account.</p>";
                        await _emailService.SendEmailAsync(user.UserName, "🚫 Account Blocked", blockEmail);
                    }
                    else if (user.FailedLoginAttempts == 4)
                    {
                        string emailBody = @"
                    <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                        <p>Dear user,</p>
                        <p><strong>Security Alert:</strong> You have entered an incorrect password <strong>4 times</strong>.</p>
                        <p>If you enter the wrong password one more time, your account will be <strong>locked</strong>.</p>
                        <br/>
                        <p>Best regards,<br/>PresCrypt Security Team</p>
                    </div>";
                        await _emailService.SendEmailAsync(user.UserName, "⚠️ Security Alert: Failed Login Attempts", emailBody);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                    return BadRequest(new { success = false, message = "Invalid email or password." });
                }

                // ✅ Reset on successful login
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginTime = null;
                user.IsBlocked = false;

                // Admin 2FA
                if (user.Role == "Admin")
                {
                    string code = new Random().Next(100000, 999999).ToString();
                    user.TwoFactorCode = code;
                    user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
                    await _applicationDbContext.SaveChangesAsync();

                    string verifyUrl = $"http://localhost:3000/Auth/Verify2FA?email={Uri.EscapeDataString(user.UserName)}";
                    string emailBody = $@"
                <p>Your 2FA code is: <strong>{code}</strong></p>
                <p>This code will expire in 5 minutes.</p>
                <p><a href='{verifyUrl}'>Click here to verify</a></p>";
                    await _emailService.SendEmailAsync(user.UserName, "Your Admin 2FA Code", emailBody);

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
                Response.Cookies.Append("authToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

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

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

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
            string emailBody = $@"
            <div style='font-family: Arial, sans-serif; font-size: 15px; color: #333;'>
                <h2>Password Reset Request</h2>
                <p>Hi,</p>
                <p>You recently requested to reset your password. Click the button below to proceed:</p>
                <p>
                    <a href='{resetLink}' style='
                        display: inline-block;
                        padding: 10px 20px;
                        color: white;
                        background-color: #007b5e;
                        text-decoration: none;
                        border-radius: 5px;
                    '>Reset Password</a>
                </p>
                <p>This link will expire in 1 hour. If you did not request this, please ignore this email.</p>
                <br/>
                <p>Thanks,<br/>PresCrypt Team</p>
            </div>";

            await _emailService.SendEmailAsync(
                email,
                "Reset Your Password",
                emailBody
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
            // If using cookie authentication  
            Response.Cookies.Delete("authToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            // Add a response header to notify the client to clear local storage  
            Response.Headers.Add("Clear-Local-Storage", "true");

            return Ok(new { message = "Logged out successfully" });
        }

    }
}