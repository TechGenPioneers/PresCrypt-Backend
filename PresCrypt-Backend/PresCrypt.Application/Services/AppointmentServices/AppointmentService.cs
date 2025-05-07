using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
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

            // Query availability with hospital information
            var availability = await _context.DoctorAvailability
                .Include(a => a.Hospital) // Include the Hospital navigation property
                .Where(a => a.DoctorId == doctorId && a.AvailableDay == dayOfWeek)
                .Select(a => new AvailabilityDisplayDto
                {
                    AvailabilityId = a.AvailabilityId,
                    DoctorId = a.DoctorId,
                    AvailableDay = a.AvailableDay,
                    AvailableStartTime = a.AvailableStartTime,
                    AvailableEndTime = a.AvailableEndTime,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName
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

        //public async Task CancelAppointmentAsync(string appointmentId)
        //{
        //    var appointment = await _context.Appointments.FindAsync(appointmentId);
        //    if (appointment == null) throw new Exception("Appointment not found");

        //    var oldStatus = appointment.Status;
        //    appointment.Status = "Cancelled";
        //    appointment.UpdatedAt = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();

        //    // Only notify if status actually changed to cancelled
        //    if (oldStatus != "Cancelled")
        //    {
        //        await _doctorNotificationService.NotifyCancelledAppointmentAsync(appointment);
        //    }
        //}

        public async Task<IEnumerable<AppointmentDisplayDto>> GetRecentAppointmentsByDoctorAsync(string doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // First get the IDs of the most recent PAST appointments per patient
            var recentAppointmentIds = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date < today) // Only past appointments
                .GroupBy(a => a.PatientId)
                .Select(g => g.OrderByDescending(a => a.Date)
                             .ThenByDescending(a => a.Time)
                             .Select(a => a.AppointmentId)
                             .FirstOrDefault())
                .Where(id => id != null) // Filter out nulls for patients without past appointments
                .ToListAsync();

            // If no past appointments found, return empty list
            if (!recentAppointmentIds.Any())
                return new List<AppointmentDisplayDto>();

            // Then fetch the complete appointment data with includes
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Hospital)
                .Where(a => recentAppointmentIds.Contains(a.AppointmentId))
                .Select(a => new AppointmentDisplayDto
                {
                    AppointmentId = a.AppointmentId,
                    Date = a.Date,
                    Time = a.Time,
                    Status = a.Status,
                    PatientId = a.Patient.PatientId,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    Gender = a.Patient.Gender,
                    DOB = a.Patient.DOB,
                    ProfileImage = a.Patient.ProfileImage
                })
                .ToListAsync();
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
        public async Task<bool> DeleteAppointmentAsync(string appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PatientAppointmentListDto>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Select(a => new PatientAppointmentListDto
                {
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    Specialization = a.Doctor.Specialization,
                    HospitalName = a.Hospital.HospitalName,
                    Time = a.Time,
                    Date = a.Date,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<List<AppointmentRescheduleDto>> GetAvailableHospitalsByDateAsync(DateTime date, string doctorId)
        {
            var dayOfWeek = date.DayOfWeek.ToString();


            var hospitals = await _context.DoctorAvailability
                .Include(a => a.Hospital)
                .Where(a => a.AvailableDay == dayOfWeek && a.DoctorId == doctorId)
                .Select(a => new AppointmentRescheduleDto
                {
                    HospitalId = a.Hospital.HospitalId,
                    HospitalName = a.Hospital.HospitalName
                })
                .Distinct()
                .ToListAsync();

            return hospitals;
        }

        public async Task<int> RescheduleAppointmentsAsync(AppointmentRescheduleDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate required fields
            if (string.IsNullOrEmpty(dto.DoctorId) ||
                string.IsNullOrEmpty(dto.HospitalId) ||
                dto.Date == default ||
                dto.Time == default)
            {
                throw new ArgumentException("Doctor, hospital, date, and time must be specified");
            }

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == dto.DoctorId
                         && a.HospitalId == dto.HospitalId
                         && a.Status == "Upcoming"
                         && (a.Date > dto.Date ||
                            (a.Date == dto.Date && a.Time >= dto.Time)))
                .ToListAsync();

            if (!appointments.Any())
            {
                throw new InvalidOperationException("No upcoming appointments found to reschedule.");
            }

            foreach (var appointment in appointments)
            {
                appointment.Status = "Rescheduled";
                appointment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return appointments.Count;
        }
    }
}