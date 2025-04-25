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
                        PasswordHash = hashedPassword
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
                return BadRequest(new { message = "Email already exists." });
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
        //public IActionResult Login([FromBody] LoginDTO loginDTO)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
        //        {
        //            return BadRequest(new { message = "Username and password are required." });
        //        }

        //        string inputUsername = loginDTO.Email.Trim().ToLower();

        //        var user = _applicationDbContext.User
        //            .FirstOrDefault(u => u.UserName.ToLower() == inputUsername);

        //        if (user == null)
        //        {
        //            return BadRequest(new { success = false, message = "Invalid username or password." });
        //        }

        //        if (user.Role == "DoctorPending")
        //        {
        //            return BadRequest(new
        //            {
        //                success = false,
        //                message = "Your doctor account is pending approval. Please wait for confirmation."
        //            });
        //        }

        //        var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDTO.Password);

        //        if (result != PasswordVerificationResult.Success)
        //        {
        //            return BadRequest(new { success = false, message = "Invalid username or password." });
        //        }

        //        var token = _jwtService.GenerateToken(user.UserId, user.UserName, user.Role);

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
        public IActionResult Login([FromBody] LoginDTO loginDTO)
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

                // Check for lockout
                if (user.FailedLoginAttempts >= 5 && user.LastFailedLoginTime.HasValue &&
                    user.LastFailedLoginTime.Value.AddMinutes(15) > DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Account locked due to too many failed attempts. Try again later." });
                }

                var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDTO.Password);

                if (result != PasswordVerificationResult.Success)
                {
                    // Update failed attempt count
                    user.FailedLoginAttempts += 1;
                    user.LastFailedLoginTime = DateTime.UtcNow;
                    _applicationDbContext.SaveChanges();

                    return BadRequest(new { success = false, message = "Invalid email or password." });
                }

                // Reset failed attempts after successful login
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginTime = null;
                _applicationDbContext.SaveChanges();

                var token = _jwtService.GenerateToken(user.UserId, user.UserName, user.Role);

                // Log successful login
                _logger.LogInformation($"Successful login for {user.UserName}");

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
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
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


        [Authorize(Roles = "Admin")]
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


        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            // Normalize email
            string emailLower = model.Email.Trim().ToLower();

            // Check User table
            var user = _applicationDbContext.User.FirstOrDefault(x => x.UserName.ToLower() == emailLower);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Generate a URL-safe token
            user.ResetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            user.ResetTokenExpire = DateTime.UtcNow.AddHours(1);
            _applicationDbContext.SaveChanges();

            // Send email with reset link
            string resetLink = $"https://localhost:3000/reset-password?token={user.ResetToken}&email={model.Email}";

            await _emailService.SendEmailAsync(user.UserName, "Reset Password",
                $"Click the link to reset your password: {resetLink}");

            return Ok(new { message = "Password reset link sent to your email." });
        }


        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest(new { message = "Email, token, and new password are required." });
            }

            // Normalize email
            string emailLower = model.Email.Trim().ToLower();
            var user = _applicationDbContext.User.FirstOrDefault(x => x.UserName == emailLower);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or token." });
            }

            // Check if token is valid and not expired
            if (user.ResetToken != model.Token || user.ResetTokenExpire < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            // Validate new password
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(model.NewPassword, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters long and include an uppercase letter, a lowercase letter, a digit, and a special character." });
            }

            // Hash and update password
            user.PasswordHash = _passwordHasher.HashPassword(null, model.NewPassword);

            // Remove token after reset
            user.ResetToken = null;
            user.ResetTokenExpire = null;

            _applicationDbContext.SaveChanges();

            return Ok(new { message = "Password reset successful. You can now log in with your new password." });
        }
        [HttpGet]
        [Route("GetAllHospitals")]
        public IActionResult GetHospitals()
        {
            return Ok(_applicationDbContext.Hospitals.ToList());
        }

    }
}