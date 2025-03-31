using Microsoft.EntityFrameworkCore;
//using PresCrypt_Backend.PresCrypt.Infrastructure.Data;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
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

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsForTodayAsync(string doctorId)
        {
            var today = DateTime.Today;

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date == today)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDisplayDto
            {
                AppointmentId = a.AppointmentId,
                Time = a.Time,
                Status = a.Status,
                PatientName = a.Patient.PatientName,
                PatientId = a.Patient.PatientId,
                DoctorName = a.Doctor.DoctorName,
                Gender = a.Patient.Gender,
                DOB = a.Patient.DOB
                //ProfilePictureUrl = a.Patient.ProfilePictureUrl
            }).ToList();
        }

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsByDateAsync(string date, string doctorId)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                throw new ArgumentException("Invalid date format. Please provide a date in the format YYYY-MM-DD.");
            }

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date == parsedDate.Date)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDisplayDto
            {
                AppointmentId = a.AppointmentId,
                Time = a.Time,
                Status = a.Status,
                PatientName = a.Patient.PatientName,
                PatientId = a.Patient.PatientId,
                DoctorName = a.Doctor.DoctorName,
                Gender = a.Patient.Gender,
                DOB = a.Patient.DOB
                //ProfilePictureUrl = a.Patient.ProfilePictureUrl
            }).ToList();
        }

        public async Task<IEnumerable<AvailabilityDisplayDto>> GetAvailabilityByDateAsync(string date, string doctorId)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                throw new ArgumentException("Invalid date format. Please provide a date in the format YYYY-MM-DD.");
            }

            var dayOfWeek = parsedDate.DayOfWeek.ToString();

            var availabilities = await _context.Doctor_Availability
                .Where(a => a.DoctorId == doctorId && a.AvailableDay == dayOfWeek)
                .ToListAsync();

            return availabilities.Select(a => new AvailabilityDisplayDto
            {
                AvailabilityId = a.AvailabilityId,
                DoctorId = a.DoctorId,
                AvailableDay = a.AvailableDay,
                AvailableTime = a.AvailableTime
            }).ToList();
        }
    }

}