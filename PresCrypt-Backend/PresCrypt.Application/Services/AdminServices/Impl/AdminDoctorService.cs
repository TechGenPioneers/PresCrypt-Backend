using PresCrypt_Backend.PresCrypt.API.Dto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Diagnostics;


namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDoctorService : IAdminDoctorService
    {
        public readonly ApplicationDbContext _context;
        public AdminDoctorService() { }
        public AdminDoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminDoctorDto>> GetAllDoctor()
        {
            Debug.WriteLine("doctors");
            var doctors = await _context.Doctors
                .Select(d => new AdminDoctorDto
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Email = d.Email,
                    Specialization = d.Specialization,
                    SlmcLicense = d.SLMCRegId,
                    ProfilePhoto = d.ProfilePhoto,
                    NIC = d.NIC,
                    EmailVerified = d.EmailVerified,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    Status = d.Status,
                    LastLogin = d.LastLogin
                })
                .ToListAsync();
            Debug.WriteLine(doctors);
            return doctors;
        }

        public async Task<string> SaveDoctor(AdminDoctorDto newDoctorDto)
        {
            Debug.WriteLine("SAVEDOCTOR SERVICE \n");
            try
            {
                // Map DTO to Entity
                var newDoctor = new Doctor
                {
                    DoctorId = "D009",
                    FirstName = newDoctorDto.FirstName,
                    LastName = newDoctorDto.LastName,
                    Email = newDoctorDto.Email,
                    Specialization = newDoctorDto.Specialization,
                    SLMCRegId = newDoctorDto.SlmcLicense,
                    SLMCIdPhoto = new byte[0], // want to Implement
                    ProfilePhoto = new byte[0], // want to Implement
                    IdPhoto = new byte[0], // want to Implement
                    NIC = newDoctorDto.NIC,
                    EmailVerified = true, // want to Implement
                    CreatedAt = DateTime.Now,  // Set current date
                    UpdatedAt = null,
                    Status = true, // want to Implement
                    LastLogin = null
                };

                // Add to DbContext
                await _context.Doctors.AddAsync(newDoctor);

                // Save changes and check result
                int result = await _context.SaveChangesAsync();
                return result > 0 ? "Success" : "Error";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

        }

    }
}
