using PresCrypt_Backend.PresCrypt.API.Dto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Diagnostics;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util;


namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDoctorService : IAdminDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminDoctorUtil _adminDoctorUtil;
        public AdminDoctorService(ApplicationDbContext context,AdminDoctorUtil adminDoctorUtil)
        {
            _context = context;
            _adminDoctorUtil = adminDoctorUtil;
        }

        public async Task<List<AdminAllDoctorsDto>> GetAllDoctor()
        {
            Debug.WriteLine("doctors");
            var doctors = await _context.Doctors
                .Select(d => new AdminAllDoctorsDto
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Specialization = d.Specialization,
                    ProfilePhoto = d.ProfilePhoto
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
                
                string newDoctorId = await _adminDoctorUtil.GenerateDoctorId();

                var newDoctor = new Doctor
                {
                    DoctorId = newDoctorId,
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
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }

        }

    }
}
