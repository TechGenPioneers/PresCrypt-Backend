using PresCrypt_Backend.PresCrypt.API.Dto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Diagnostics;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util;
using System.Linq;


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
        public async Task<List<HospitalDto>> getAllHospitals()
        {
            var hospitals = await _context.Hospitals
                .Select(d => new HospitalDto
                {
                    HospitalId = d.HospitalId,
                    HospitalName = d.HospitalName
                })
                .ToListAsync();
            return hospitals;
        }
        public async Task<List<AdminAllDoctorsDto>> GetAllDoctor()
        {
        //    Debug.WriteLine("doctors");
            var doctors = await _context.Doctors
                .Select(d => new AdminAllDoctorsDto
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender=d.Gender,
                    Specialization = d.Specialization,
                    ProfilePhoto = d.ProfilePhoto
                })
                .ToListAsync();
            //Debug.WriteLine(doctors);
            return doctors;
        }

        public async Task<string> SaveDoctor(DoctorAvailabilityDto newDoctorDto)
        {
            try
            {
                
                string newDoctorId = await _adminDoctorUtil.GenerateDoctorId();

                var newDoctor = new Doctor
                {
                    DoctorId = newDoctorId,
                    FirstName = newDoctorDto.Doctor.FirstName,
                    LastName = newDoctorDto.Doctor.LastName,
                    Gender = newDoctorDto.Doctor.Gender,
                    Email = newDoctorDto.Doctor.Email,
                    Specialization = newDoctorDto.Doctor.Specialization,
                    ContactNumber = newDoctorDto.Doctor.ContactNumber,
                    Description = newDoctorDto.Doctor.Description,
                    SLMCRegId = newDoctorDto.Doctor.SlmcLicense,
                    SLMCIdPhoto = new byte[0], // want to Implement
                    ProfilePhoto = new byte[0], // want to Implement
                    IdPhoto = new byte[0], // want to Implement
                    NIC = newDoctorDto.Doctor.NIC,
                    EmailVerified = true, // want to Implement
                    CreatedAt = DateTime.Now,  // Set current date
                    UpdatedAt = null,
                    Status = true, // want to Implement
                    LastLogin = null
                };

                // Add to DbContext
                await _context.Doctors.AddAsync(newDoctor);

                // Now, save the doctor's availability
                foreach (var availability in newDoctorDto.Availability)
                {
                    var newAvailability = new Doctor_Availability
                    {
                        DoctorId = newDoctorId,
                        AvailableDay = availability.Day, 
                        AvailableStartTime = TimeOnly.Parse(availability.StartTime),
                        AvailableEndTime = TimeOnly.Parse(availability.EndTime),
                        HospitalId = availability.HospitalId
                    };

                    // Add Availability to DbContext
                    await _context.Doctor_Availability.AddAsync(newAvailability);
                }


                // Save changes and check result
                int result = await _context.SaveChangesAsync();
                return result > 0 ? "Success" : "Error";
            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }

        }

        public async Task<DoctorAvailabilityDto> getDoctorById(string doctorID)
        {
            // Fetch doctor details
            var getDoctor = await _context.Doctors
                .Where(d => d.DoctorId == doctorID)
                .Select(d => new AdminDoctorDto
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    ProfilePhoto = d.ProfilePhoto,
                    Email = d.Email,
                    Specialization = d.Specialization,
                    SlmcLicense = d.SLMCRegId,
                    NIC = d.NIC,
                    Description = d.Description,
                    EmailVerified = d.EmailVerified,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    LastLogin = d.LastLogin,
                    ContactNumber = d.ContactNumber
                })
                .FirstOrDefaultAsync();

            // If doctor not found, return null
            if (getDoctor != null)
            {

                // Fetch doctor availability along with hospital names
                var getDoctorAvailability = await _context.Doctor_Availability
                    .Where(d => d.DoctorId == doctorID)
                    .Join(
                        _context.Hospitals,
                        a => a.HospitalId,// Foreign key in Doctor_Availability
                        h => h.HospitalId,// Primary key in Hospital tablev
                        (a, h) => new AvailabilityDto
                        {
                            Day = a.AvailableDay,
                            StartTime = a.AvailableStartTime.ToString(),
                            EndTime = a.AvailableEndTime.ToString(),
                            HospitalName = h.HospitalName,
                            HospitalId = h.HospitalId
                        }
                    )
                    .ToListAsync();

                // Combine doctor details and availability
                var doctorAndAvailability = new DoctorAvailabilityDto()
                {
                    Doctor = getDoctor,
                    Availability = getDoctorAvailability
                };
                return doctorAndAvailability;
            }
            else
            {
                return null;
            }

        }

    }
}
