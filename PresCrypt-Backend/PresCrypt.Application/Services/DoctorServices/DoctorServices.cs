using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices
{
    public class DoctorServices : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorSearchDto>> GetDoctorAsync(string specialization, string hospitalName)
        {
            var doctors = await _context.Doctor
                .Join(
                    _context.Doctor_Availability,
                    doctor => doctor.DoctorId,
                    availability => availability.DoctorId,
                    (doctor, availability) => new { doctor, availability }
                )
                .Join(
                    _context.Hospitals,
                    da => da.availability.HospitalId,
                    hospital => hospital.HospitalId,
                    (da, hospital) => new { da.doctor, da.availability, hospital }
                )
                .Where(dh =>
                    (string.IsNullOrEmpty(specialization) || dh.doctor.Specialization.Contains(specialization)) &&
                    (string.IsNullOrEmpty(hospitalName) || dh.hospital.HospitalName.Contains(hospitalName))
                )
                .Select(dh => new DoctorSearchDto
                {
                    DoctorId = dh.doctor.DoctorId,
                    DoctorName = dh.doctor.DoctorName,
                    AvailableDates = new List<DateTime> { dh.availability.AvailableDate.ToDateTime(TimeOnly.MinValue) },
                    AvailableTimes = new List<TimeSpan> { dh.availability.AvailableTime.ToTimeSpan() },
                    Charge = dh.hospital.Charge // Include the hospital's charge here
                })
                .ToListAsync();

            return doctors;
        }

    }
}