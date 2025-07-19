using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices
{
    public class DoctorServices : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorServices(ApplicationDbContext context)
        {
            _context = context;
        }
     
        public async Task<List<DoctorSearchDto>> GetDoctorAsync(string specialization, string hospitalName, string name)
        {
            var query = _context.Doctor
                .Join(
                    _context.DoctorAvailability,
                    doctor => doctor.DoctorId,
                    availability => availability.DoctorId,
                    (doctor, availability) => new { doctor, availability }
                )
                .Join(
                    _context.Hospitals,
                    da => da.availability.HospitalId,
                    hospital => hospital.HospitalId,
                    (da, hospital) => new { da.doctor, da.availability, hospital }
                );

            // Priority logic — name has higher priority
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(dh =>
                    (dh.doctor.FirstName + " " + dh.doctor.LastName).Contains(name) ||
                    dh.doctor.FirstName.Contains(name) ||
                    dh.doctor.LastName.Contains(name)
                );
            }
            else
            {
                // Apply specialization and hospital filters only if name is not provided
                query = query.Where(dh =>
                    (string.IsNullOrEmpty(specialization) || dh.doctor.Specialization.Contains(specialization)) &&
                    (string.IsNullOrEmpty(hospitalName) || dh.hospital.HospitalName.Contains(hospitalName))
                );
            }

            // Project into DTO
            var result = await query
                .Select(dh => new DoctorSearchDto
                {
                    DoctorId = dh.doctor.DoctorId,
                    HospitalId = dh.hospital.HospitalId,
                    FirstName = dh.doctor.FirstName,
                    LastName = dh.doctor.LastName,
                    Specialization = dh.doctor.Specialization,
                    HospitalName = dh.hospital.HospitalName,
                    Charge = dh.hospital.Charge, // Use hospital's charge
                    Image = dh.doctor.DoctorImage,

                    // Availability details as lists
                    AvailableDay = new List<string> { dh.availability.AvailableDay },
                    AvailableTime = new List<TimeSpan> { dh.availability.AvailableStartTime.ToTimeSpan() }
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<string>> GetAllSpecializationsAsync()
        {
            return await _context.Doctor
                .AsNoTracking()//uses not to track object by .net
                .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
                .Select(d => d.Specialization.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllDoctor()
        {
            return await _context.Doctor
                .AsNoTracking()
                .Select(d => d.FirstName + " " + d.LastName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetDoctorAvailabilityByNameAsync(string doctorName)
        {
            if (string.IsNullOrWhiteSpace(doctorName))
                return Enumerable.Empty<object>();

            var lowerName = doctorName.ToLower();

            var data = await _context.Doctor
                .Where(d => (d.FirstName + " " + d.LastName).ToLower().Contains(lowerName))
                .Include(d => d.Availabilities)
                    .ThenInclude(a => a.Hospital)
                .SelectMany(d => d.Availabilities.Select(a => new
                {
                    DoctorName = d.FirstName + " " + d.LastName,
                    a.AvailableDay,
                    a.AvailableStartTime,
                    a.AvailableEndTime,
                    HospitalName = a.Hospital.HospitalName
                }))
                .ToListAsync();

            return data;
        }
    }
}