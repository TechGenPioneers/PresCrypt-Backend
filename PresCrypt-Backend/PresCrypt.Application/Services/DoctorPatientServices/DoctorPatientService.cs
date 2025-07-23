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

        public async Task<IEnumerable<DoctorPatientDto>> GetPatientDetailsAsync(
            string doctorId,
            string type = "past",
            string? hospitalName = null)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            // Base query filtered by doctor
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId);

            // Filter by Hospital Name if provided
            if (!string.IsNullOrEmpty(hospitalName))
            {
                query = query.Where(a => a.Hospital.HospitalName == hospitalName);
            }

            // Project needed fields before grouping
            var projectedQuery = query
                .Select(a => new
                {
                    a.AppointmentId,
                    a.Date,
                    a.Time,
                    a.Status,
                    a.PatientId,
                    a.HospitalId,
                    PatientFirstName = a.Patient.FirstName,
                    PatientLastName = a.Patient.LastName,
                    a.Patient.Gender,
                    a.Patient.DOB,
                    a.Patient.ProfileImage,
                    HospitalName = a.Hospital.HospitalName
                });

            // Group by patient
            var groupedAppointments = await projectedQuery
                .GroupBy(a => a.PatientId)
                .ToListAsync();

            var filteredAppointments = new List<dynamic>();

            foreach (var group in groupedAppointments)
            {
                var hasPastAppointment = group.Any(a =>
                    a.Date < currentDate || (a.Date == currentDate && a.Time <= currentTime));

                if ((type == "past" && hasPastAppointment) ||
                    (type == "new" && !hasPastAppointment))
                {
                    var latestAppointment = group
                        .OrderByDescending(a => a.Date)
                        .ThenByDescending(a => a.Time)
                        .FirstOrDefault();

                    if (latestAppointment != null)
                    {
                        filteredAppointments.Add(latestAppointment);
                    }
                }
            }

            // Map to DTO
            return filteredAppointments.Select(a => new DoctorPatientDto
            {
                AppointmentId = a.AppointmentId,
                Date = a.Date.ToDateTime(TimeOnly.MinValue),
                Time = a.Time,
                Status = a.Status,
                PatientId = a.PatientId,
                HospitalId = a.HospitalId,
                HospitalName = a.HospitalName,
                PatientName = $"{a.PatientFirstName} {a.PatientLastName}",
                Gender = a.Gender,
                DOB = a.DOB,
                ProfileImage = a.ProfileImage
            });
        }
    }
}
