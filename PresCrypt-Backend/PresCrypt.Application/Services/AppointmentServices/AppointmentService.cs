using Microsoft.EntityFrameworkCore;
//using PresCrypt_Backend.PresCrypt.Infrastructure.Data;
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

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsForTodayAsync(string doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

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
                PatientName = a.Patient.FirstName,
                PatientId = a.Patient.PatientId,
                DoctorName = a.Doctor.FirstName,
                Gender = a.Patient.Gender,
                DOB = a.Patient.DOB
                //ProfilePictureUrl = a.Patient.ProfilePictureUrl
            }).ToList();
        }

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsByDateAsync(string date, string doctorId)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                throw new ArgumentException("Invalid date format. Please provide a date in the format YYYY-MM-DD.");
            }

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date == parsedDate)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDisplayDto
            {
                AppointmentId = a.AppointmentId,
                Time = a.Time,
                Status = a.Status,
                PatientName = a.Patient.FirstName,
                PatientId = a.Patient.PatientId,
                DoctorName = a.Doctor.FirstName,
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

            var availabilities = await _context.DoctorAvailability
                .Where(a => a.DoctorId == doctorId && a.AvailableDay == dayOfWeek)
                .ToListAsync();

            return availabilities.Select(a => new AvailabilityDisplayDto
            {
                AvailabilityId = int.Parse(a.AvailabilityId), // Fix: Convert string to int
                DoctorId = a.DoctorId,
                AvailableDay = a.AvailableDay,
                AvailableTime = a.AvailableStartTime
            }).ToList();
        }

        public async Task<Appointment> CreateAppointmentAsync(AppointmentSave dto)
        {
            var appointment = dto.Adapt<Appointment>();
            appointment.CreatedAt = DateTime.UtcNow;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;

        }
    }

}