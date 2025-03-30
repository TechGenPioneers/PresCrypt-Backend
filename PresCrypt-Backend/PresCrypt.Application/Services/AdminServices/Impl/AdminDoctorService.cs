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
                    NIC = newDoctorDto.Doctor.NIC,
                    EmailVerified = true, // want to Implement
                    CreatedAt = DateTime.Now,  // Set current date
                    UpdatedAt = DateTime.Now,
                    Status = true, // want to Implement
                    LastLogin = null
                };

                // Add to DbContext
                await _context.Doctors.AddAsync(newDoctor);

                int result = 0;
                foreach (var availability in newDoctorDto.Availability)
                {
                    string newAvailabilityId = await _adminDoctorUtil.GenerateAvailabilityId();
                    Debug.WriteLine(newAvailabilityId);

                    var newAvailability = new Doctor_Availability
                    {
                        AvailabilityId = newAvailabilityId,
                        DoctorId = newDoctorId,
                        AvailableDay = availability.Day,
                        AvailableStartTime = TimeOnly.Parse(availability.StartTime),
                        AvailableEndTime = TimeOnly.Parse(availability.EndTime),
                        HospitalId = availability.HospitalId
                    };

                    await _context.Doctor_Availability.AddRangeAsync(newAvailability);
                    result = await _context.SaveChangesAsync();
                }

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
                    CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,

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
                            AvailabilityId = a.AvailabilityId.ToString(),
                            Day = a.AvailableDay,
                            StartTime = a.AvailableStartTime.ToString("HH:mm"),  // Formats as HH:mm
                            EndTime = a.AvailableEndTime.ToString("HH:mm"), 
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

       public async Task<string> UpdateDoctor(DoctorAvailabilityDto dto)
{
    try
    {
        if (dto == null || dto.Doctor == null)
        {
            return "Invalid input data";
        }

        var getDoctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == dto.Doctor.DoctorId);

        if (getDoctor == null)
        {
            return "Doctor not found";
        }

        // Update only if values are changed
        getDoctor.FirstName = dto.Doctor.FirstName ?? getDoctor.FirstName;
        getDoctor.LastName = dto.Doctor.LastName ?? getDoctor.LastName;
        getDoctor.Gender = dto.Doctor.Gender ?? getDoctor.Gender;
        getDoctor.Email = dto.Doctor.Email ?? getDoctor.Email;
        getDoctor.Specialization = dto.Doctor.Specialization ?? getDoctor.Specialization;
        getDoctor.SLMCRegId = dto.Doctor.SlmcLicense ?? getDoctor.SLMCRegId;
        getDoctor.NIC = dto.Doctor.NIC ?? getDoctor.NIC;
        getDoctor.Description = dto.Doctor.Description ?? getDoctor.Description;
        getDoctor.Status = dto.Doctor.Status ?? getDoctor.Status;
        getDoctor.UpdatedAt = DateTime.Now;
        getDoctor.ContactNumber = dto.Doctor.ContactNumber ?? getDoctor.ContactNumber;

        // Fetch existing availabilities
        var existingAvailabilities = await _context.Doctor_Availability
            .Where(a => a.DoctorId == dto.Doctor.DoctorId)
            .ToListAsync();

        var newAvailabilityIds = dto.Availability
            .Where(a => a.AvailabilityId != null)
            .Select(a => a.AvailabilityId)
            .ToHashSet();

        // DELETE: Remove availabilities not in the new list
        var availabilitiesToDelete = existingAvailabilities
            .Where(a => !string.IsNullOrEmpty(a.AvailabilityId) && !newAvailabilityIds.Contains(a.AvailabilityId))
            .ToList();

        if (availabilitiesToDelete.Any())
        {
            _context.Doctor_Availability.RemoveRange(availabilitiesToDelete);
        }

                int result = 0;
        foreach (var availability in dto.Availability.Where(a => a.AvailabilityId == null))
        {
                    
                    result =  await _context.SaveChangesAsync();
                    string newAvailabilityId = await _adminDoctorUtil.GenerateAvailabilityId();
            Debug.WriteLine(newAvailabilityId);
           var newAvailabilities = new Doctor_Availability
            {
                AvailabilityId = newAvailabilityId,
                DoctorId = dto.Doctor.DoctorId,
                AvailableDay = availability.Day,
                AvailableStartTime = TimeOnly.Parse(availability.StartTime),
                AvailableEndTime = TimeOnly.Parse(availability.EndTime),
                HospitalId = availability.HospitalId
            };
                    await _context.Doctor_Availability.AddRangeAsync(newAvailabilities);

                }
                result =  await _context.SaveChangesAsync();
                return result > 0 ? "Success" : "Error";
    }
    catch (DbUpdateException ex)
    {
        return $"Database update error: {ex.Message} \nStackTrace: {ex.StackTrace}";
    }
    catch (Exception e)
    {
        return $"Unexpected error: {e.Message} \nStackTrace: {e.StackTrace}";
    }
}

        public async Task<string> deleteDoctorById(string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return "doctorId is null";
            }

            try
            {
                // Fetch the doctor
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

                if (doctor == null)
                {
                    return "Doctor not found";
                }

                // Remove doctor from the database
                _context.Doctors.Remove(doctor);

                // Remove related doctor availability records
                var doctorAvailabilities = _context.Doctor_Availability
                    .Where(a => a.DoctorId == doctorId);
                _context.Doctor_Availability.RemoveRange(doctorAvailabilities);

                // Save changes
                await _context.SaveChangesAsync();

                return "Doctor deleted successfully";
            }
            catch (Exception e)
            {
                return $"Unexpected error: {e.Message} \nStackTrace: {e.StackTrace}";
            }
        }

    }
}
