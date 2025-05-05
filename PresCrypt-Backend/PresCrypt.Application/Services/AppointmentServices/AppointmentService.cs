using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsAsync(string doctorId, DateOnly? date = null)
        {
            // Get current date at server's timezone
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            // Start building the query
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => a.DoctorId == doctorId);

            // Apply date filter if specified, otherwise get future appointments
            query = date.HasValue
                ? query.Where(a => a.Date == date.Value)
                : query.Where(a => a.Date >= currentDate);

            // Execute and map to DTO
            return await query
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .Select(a => new AppointmentDisplayDto
                {
                    AppointmentId = a.AppointmentId,
                    Date = a.Date,  // Ensure consistent date format
                    Time = a.Time, // Format time consistently
                    Status = a.Status,
                    PatientId = a.Patient.PatientId,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    Gender = a.Patient.Gender,
                    DOB = a.Patient.DOB, // Format DOB consistently
                    ProfileImage = a.Patient.ProfileImage
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AvailabilityDisplayDto>> GetAvailabilityByDateAsync(string date, string doctorId)
        {
            // Validate date format
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");
            }

            // Get day of week (e.g., "Monday")
            var dayOfWeek = parsedDate.DayOfWeek.ToString();

            // Query availability
            var availability = await _context.DoctorAvailability

                .Where(a => a.DoctorId == doctorId && a.AvailableDay == dayOfWeek)
                .Select(a => new AvailabilityDisplayDto
                {
                    AvailabilityId = a.AvailabilityId,
                    DoctorId = a.DoctorId,
                    AvailableDay = a.AvailableDay,
                    AvailableStartTime = a.AvailableStartTime,
                    AvailableEndTime = a.AvailableEndTime
                })
                .ToListAsync();

            return availability;
        }

        public async Task<Appointment> CreateAppointmentAsync(AppointmentSave dto)
        {
            var appointment = dto.Adapt<Appointment>();
            appointment.CreatedAt = DateTime.UtcNow;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;

        }
        public async Task<Dictionary<DateTime, int>> GetAppointmentCountsAsync(string doctorId, List<DateTime> dates)
        {
            var result = new Dictionary<DateTime, int>();

            foreach (var date in dates)
            {
                var count = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && a.Date == DateOnly.FromDateTime(date))
                    .CountAsync();

                result[date] = count;
            }

            return result;
        }

        public async Task<List<PatientAppointmentListDto>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Select(a => new PatientAppointmentListDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    PatientEmail = a.Patient.Email,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    DoctorEmail= a.Doctor.Email,
                    Specialization = a.Doctor.Specialization,
                    HospitalName = a.Hospital.HospitalName,
                    Time = a.Time,
                    Date = a.Date,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteAppointmentAsync(string appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}