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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Hospital = PresCrypt_Backend.PresCrypt.Core.Models.Hospital;

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

        public UserController(ApplicationDbContext applicationDbContext, IEmailService emailService, ILogger<UserController> logger)
        {
            _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
            _passwordHasher = new PasswordHasher<User>();
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route("PatientRegistration")]
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

            // Check if the email already exists in the User table
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
                    // Step 2: Insert into User table first
                    var newUser = new User
                    {
                        UserId = newUserId,
                        UserName = emailLower,
                        PasswordHash = hashedPassword,  // ✅ Hashed password
                        Role = "Patient",
                        Patient = new List<Patient>(),
                        Doctor = new List<Doctor>(),
                        Admin = new List<Admin>()
                    };

                    _applicationDbContext.User.Add(newUser);
                    _applicationDbContext.SaveChanges();

                    // Step 3: Generate PatientId
                    var lastPatient = _applicationDbContext.Patient.OrderByDescending(p => p.PatientId).FirstOrDefault();
                    int newIdPatient = lastPatient != null && int.TryParse(lastPatient.PatientId.Substring(1), out int lastIdPatient) ? lastIdPatient + 1 : 1;
                    string newPatientId = $"P{newIdPatient:D3}";

                    // Step 4: Insert into Patient table using Email as FK
                    var newPatient = new Patient
                    {
                        PatientId = newPatientId,
                        FirstName = patientRegDTO.FirstName,
                        LastName = patientRegDTO.LastName,
                        Email = newUser.UserName,  // ✅ Use Email as FK
                        ContactNo = patientRegDTO.ContactNumber,
                        NIC = patientRegDTO.NIC,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = patientRegDTO.Status,
                        PasswordHash = hashedPassword
                    };

                    _applicationDbContext.Patient.Add(newPatient);
                    _applicationDbContext.SaveChanges();

                    transaction.Commit(); // Commit both inserts

                    return Ok(new { message = "Patient registered successfully", patientId = newPatientId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback if any issue occurs
                    return BadRequest(new { message = "Registration failed", error = ex.Message });
                }
            }
        }


        
        [HttpPost]
        [Route("DoctorRegistration")]
        public async Task<IActionResult> RegisterDoctor([FromForm] DoctorRegDTO doctorRegDTO)
        {
            // Check if model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            // Ensure SLMC registration image is provided
            if (doctorRegDTO.SLMCIdImage == null || doctorRegDTO.SLMCIdImage.Length == 0)
            {
                return BadRequest(new { message = "SLMC Registration Image is required." });
            }

            // Ensure hospital schedules are provided
            if (string.IsNullOrEmpty(doctorRegDTO.hospitalSchedules))
            {
                return BadRequest("Hospital schedules are required.");
            }

            // Deserialize hospital schedules
            List<HospitalScheduleDTO> hospitalSchedules = JsonConvert.DeserializeObject<List<HospitalScheduleDTO>>(doctorRegDTO.hospitalSchedules);

            byte[] slmcImageBytes;
            using (var ms = new MemoryStream())
            {
                await doctorRegDTO.SLMCIdImage.CopyToAsync(ms);
                slmcImageBytes = ms.ToArray();
            }

            // Validate password format
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            if (!Regex.IsMatch(doctorRegDTO.Password, passwordPattern))
            {
                return BadRequest(new { message = "Password must be at least 6 characters with 1 uppercase, 1 lowercase, 1 number, and 1 special character." });
            }

            // Check if passwords match
            if (doctorRegDTO.Password != doctorRegDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            string emailLower = doctorRegDTO.Email.Trim().ToLower();

            // Check if the email already exists
            if (await _applicationDbContext.User.AnyAsync(x => x.UserName == emailLower))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            // Begin transaction
            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create User
                string hashedPassword = _passwordHasher.HashPassword(null, doctorRegDTO.Password);
                var lastUser = await _applicationDbContext.User
                    .OrderByDescending(p => p.UserId)
                    .FirstOrDefaultAsync();

                int newId = lastUser != null && int.TryParse(lastUser.UserId.Substring(1), out int lastId)
                    ? lastId + 1 : 1;
                string newUserId = $"U{newId:D3}";

                var newUser = new User
                {
                    UserId = newUserId,
                    UserName = emailLower,
                    PasswordHash = hashedPassword,
                    Role = "DoctorPending",  // Set to "DoctorPending" as status
                    Patient = new List<Patient>(),
                    Doctor = new List<Doctor>(),
                    Admin = new List<Admin>()
                };

                await _applicationDbContext.User.AddAsync(newUser);

                // Step 2: Create DoctorRequest
                var lastRequest = await _applicationDbContext.DoctorRequest
                    .OrderByDescending(d => d.RequestId)
                    .FirstOrDefaultAsync();

                int newRequestId = lastRequest != null &&
                    int.TryParse(lastRequest.RequestId.Substring(2), out int lastReqId)
                    ? lastReqId + 1 : 1;
                string newDoctorRequestId = $"DR{newRequestId:D3}";

                var doctorRequest = new DoctorRequest
                {
                    RequestId = newDoctorRequestId,
                    FirstName = doctorRegDTO.FirstName,
                    LastName = doctorRegDTO.LastName,
                    Gender = doctorRegDTO.Gender,
                    Email = emailLower,
                    ContactNo = doctorRegDTO.ContactNumber,
                    Specialization = doctorRegDTO.Specialization,
                    SLMCRegId = doctorRegDTO.SLMCRegId,
                    SLMCIdImage = slmcImageBytes,
                    NIC = doctorRegDTO.NIC,
                    Charge = doctorRegDTO.Charge,
                    RequestStatus = "Pending", // Pending status as the doctor is being reviewed
                    EmailVerified = false
                };

                await _applicationDbContext.DoctorRequest.AddAsync(doctorRequest);

                // Step 3: Handle Hospital Schedules and Availability
                var availabilityRequests = new List<RequestAvailability>();
                int availabilityCounter = 1;

                foreach (var schedule in hospitalSchedules)
                {
                    if (schedule.availability == null) continue;

                    var hospital = await _applicationDbContext.Hospitals
                        .FirstOrDefaultAsync(h => h.HospitalId == schedule.hospitalId);
                    if (hospital == null) continue;

                    foreach (var dayEntry in schedule.availability)
                    {
                        var day = dayEntry.Key;
                        var time = dayEntry.Value;

                        if (time == null) continue;

                        if (TimeOnly.TryParse(time.startTime, out var startTime) &&
                            TimeOnly.TryParse(time.endTime, out var endTime))
                        {
                            availabilityRequests.Add(new RequestAvailability
                            {
                                AvailabilityRequestId = $"AR{availabilityCounter++}",
                                DoctorRequestId = doctorRequest.RequestId,
                                AvailableDay = day,
                                AvailableStartTime = startTime,
                                AvailableEndTime = endTime,
                                HospitalId = hospital.HospitalId
                            });
                        }
                    }
                }

                // Step 4: Save Availability Requests
                if (availabilityRequests.Any())
                {
                    await _applicationDbContext.RequestAvailability.AddRangeAsync(availabilityRequests);
                }

                // Step 5: Save Changes
                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Registration successful",
                    requestId = doctorRequest.RequestId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Doctor registration failed.");
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
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
                        LastName=adminRegDTO.LastName,
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
        public IActionResult Login([FromBody] LoginDTO loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            // Normalize email
            string emailLower = loginDTO.Email.Trim().ToLower();

            // Check User table for the role
            var user = _applicationDbContext.User.FirstOrDefault(x => x.UserName.ToLower() == emailLower);
            if (user != null && user.Role == "DoctorPending")
            {
                return BadRequest(new { message = "Your doctor account is pending approval. Please wait for confirmation." });
            }

            // Check Patient table
            var patient = _applicationDbContext.Patient.FirstOrDefault(x => x.Email.ToLower() == emailLower);
            if (patient != null)
            {
                return HandleLogin(patient, loginDTO.Password, "Patient");
            }

            // Check Doctor table
            var doctor = _applicationDbContext.Doctor.FirstOrDefault(x => x.Email.ToLower() == emailLower);
            if (doctor != null)
            {
                return HandleLogin(doctor, loginDTO.Password, "Doctor");
            }

            // Check Admin table
            var admin = _applicationDbContext.Admin.FirstOrDefault(x => x.Email.ToLower() == emailLower);
            if (admin != null)
            {
                return HandleLogin(admin, loginDTO.Password, "Admin");
            }

            return BadRequest(new { message = "Invalid email or password." });
        }






        private IActionResult HandleLogin(Patient patient, string password, string role)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, patient.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                success = true,
                message = $"{role} login successful",
                user = new
                {
                    id = patient.PatientId,
                    email = patient.Email,
                    role = role
                }
            });
        }

        private IActionResult HandleLogin(Doctor doctor, string password, string role)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, doctor.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                success = true,
                message = $"{role} login successful",
                user = new
                {
                    id = doctor.DoctorId,
                    email = doctor.Email,
                    role = role
                }
            });
        }

        private IActionResult HandleLogin(Admin admin, string password, string role)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, admin.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                success = true,
                message = $"{role} login successful",
                user = new
                {
                    id = admin.AdminId,
                    email = admin.Email,
                    role = role
                }
            });
        }


       
        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetUsers()
        {
            return Ok(_applicationDbContext.User.ToList());
        }
        [HttpGet]
        [Route("GetAllPatients")]
        public IActionResult GetPatients()
        {
            return Ok(_applicationDbContext.Patient.ToList());
        }
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
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            // Save token in DB
            user.ResetToken = token;
            user.ResetTokenExpire= DateTime.UtcNow.AddHours(1);
            _applicationDbContext.SaveChanges();

            // Send email with reset link
            string resetLink = $"https://localhost:3000/reset-password?token={token}&email={model.Email}";

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