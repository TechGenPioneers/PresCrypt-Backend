using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices
{
    public class DoctorPatientService : IDoctorPatientService
    {
        private readonly ApplicationDbContext _context;

        public DoctorPatientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorPatientDto>> GetPatientDetailsAsync(string doctorId)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            // Build query with filter for past or current appointments only
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => a.DoctorId == doctorId)
                .Where(a => a.Date < currentDate || (a.Date == currentDate && a.Time <= currentTime)); // filter for past

            // Group by PatientId and select the latest appointment from the past
            var latestAppointments = await query
                .GroupBy(a => a.PatientId)
                .Select(g => g.OrderByDescending(a => a.Date).ThenByDescending(a => a.Time).FirstOrDefault())
                .ToListAsync();

            return latestAppointments.Select(a => new DoctorPatientDto
            {
                AppointmentId = a.AppointmentId,
                Date = a.Date.ToDateTime(TimeOnly.MinValue),
                Time = a.Time,
                Status = a.Status,
                PatientId = a.Patient.PatientId,
                HospitalId = a.HospitalId,
                HospitalName = a.Hospital.HospitalName,
                PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                Gender = a.Patient.Gender,
                DOB = a.Patient.DOB,
                ProfileImage = a.Patient.ProfileImage
            }).ToList();
        }
    }
}