using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.UserServices
{
    public class UserServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserServices(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> RegisterUserAsync(UserRegDTO userRegDTO)
        {
            if (_context.User.Any(x => x.UserName.Equals(userRegDTO.UserName, StringComparison.OrdinalIgnoreCase)))
            {
                return "Email already exists.";
            }

            // Generate User ID (Uxxx)  
            var lastUser = await _context.User.OrderByDescending(u => u.UserId).FirstOrDefaultAsync();
            int newUserIdNum = lastUser != null && int.TryParse(lastUser.UserId.Substring(1), out int lastId) ? lastId + 1 : 1;
            string newUserId = $"U{newUserIdNum:D3}";

            var newUser = new User
            {
                UserId = newUserId,
                UserName = userRegDTO.UserName,
                PasswordHash = _passwordHasher.HashPassword(null, userRegDTO.Password),
                Role = userRegDTO.Role,
                Patient = new List<Patient>(),
                Doctor = new List<Doctor>(),
                Admin = new List<Admin>()
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            // Register in respective tables  
            if (userRegDTO.Role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                await RegisterPatientAsync(newUserId);
            }
            else if (userRegDTO.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                await RegisterDoctorAsync(newUserId);
            }
            else if (userRegDTO.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                await RegisterAdminAsync(newUserId);
            }

            return newUserId;
        }


        private async Task RegisterPatientAsync(string userId)
        {
            var lastPatient = await _context.Patient.OrderByDescending(p => p.PatientId).FirstOrDefaultAsync();
            int newPatientIdNum = lastPatient != null && int.TryParse(lastPatient.PatientId.Substring(1), out int lastId) ? lastId + 1 : 1;
            string newPatientId = $"P{newPatientIdNum:D3}";

            var patient = new Patient
            {
                PatientId = newPatientId,
                CreatedAt = DateTime.UtcNow,
                FirstName = "Default Name",
                LastName = "Default Last Name",
            };

            _context.Patient.Add(patient);
            await _context.SaveChangesAsync();
        }

        private async Task RegisterDoctorAsync(string userId)
        {
            var lastDoctor = await _context.Doctor.OrderByDescending(d => d.DoctorId).FirstOrDefaultAsync();
            int newDoctorIdNum = lastDoctor != null && int.TryParse(lastDoctor.DoctorId.Substring(1), out int lastId) ? lastId + 1 : 1;
            string newDoctorId = $"D{newDoctorIdNum:D3}";

            var doctor = new Doctor
            {
                DoctorId = newDoctorId,
                
                CreatedAt = DateTime.UtcNow
            };

            _context.Doctor.Add(doctor);
            await _context.SaveChangesAsync();
        }

        private async Task RegisterAdminAsync(string userId)
        {
            var lastAdmin = await _context.Admin.OrderByDescending(a => a.AdminId).FirstOrDefaultAsync();
            int newAdminIdNum = lastAdmin != null && int.TryParse(lastAdmin.AdminId.Substring(1), out int lastId) ? lastId + 1 : 1;
            string newAdminId = $"A{newAdminIdNum:D3}";

            var admin = new Admin
            {
                AdminId = newAdminId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Admin.Add(admin);
            await _context.SaveChangesAsync();
        }

        internal async Task<string> RegisterPatientAsync(UserRegDTO userRegDTO)
        {
            throw new NotImplementedException();
        }

        internal async Task<string> RegisterDoctorAsync(UserRegDTO userRegDTO)
        {
            throw new NotImplementedException();
        }

        internal async Task<string> RegisterAdminAsync(UserRegDTO userRegDTO)
        {
            throw new NotImplementedException();
        }
    }

}
