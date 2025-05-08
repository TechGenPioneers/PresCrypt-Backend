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

        public async Task<IEnumerable<DoctorPatientDto>> GetPatientDetailsAsync(string doctorId, string type = "past")
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            // Build base query to filter by doctorId
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => a.DoctorId == doctorId);

            // Filter by appointment type (past or future)
            if (type == "past")
            {
                // Filter past appointments (appointments before today or today but before current time)
                query = query.Where(a => a.Date < currentDate || (a.Date == currentDate && a.Time <= currentTime));
            }
            else if (type == "future")
            {
                // Get all patients having only future appointments
                var patientsWithPastAppointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && (a.Date < currentDate || (a.Date == currentDate && a.Time <= currentTime)))
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToListAsync();

                query = query
                    .Where(a => !patientsWithPastAppointments.Contains(a.PatientId))  // Patients with no past appointments
                    .Where(a => a.Date > currentDate || (a.Date == currentDate && a.Time > currentTime));  // Future appointments
            }

            //  Group by PatientId to get the latest appointment (last visit)
            var latestAppointments = await query
                .GroupBy(a => a.PatientId)
                .Select(g => g.OrderByDescending(a => a.Date).ThenByDescending(a => a.Time).FirstOrDefault())
                .ToListAsync();

            // Map to DTO (DoctorPatientDto)
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
