using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;

public class DoctorDashboardService : IDoctorDashboardService
{
    private readonly ApplicationDbContext _context;

    public DoctorDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorDashboardDto> GetDashboardStatsAsync(string doctorId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        // Get both counts in a single database query for better performance
        var appointmentCounts = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       (a.Date > today || (a.Date == today && a.Time > now)))
            .GroupBy(a => a.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Get count of unique patients with upcoming appointments
        var bookedPatientsCount = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       (a.Date > today || (a.Date == today && a.Time > now)) &&
                       a.Status == "Upcoming" || a.Status == "Completed")
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        return new DoctorDashboardDto
        {
            UpcomingAppointments = appointmentCounts.FirstOrDefault(x => x.Status == "Upcoming")?.Count ?? 0,
            CancelledAppointments = appointmentCounts.FirstOrDefault(x => x.Status == "Cancelled")?.Count ?? 0,
            BookedPatients = bookedPatientsCount
        };
    }

    public async Task<DoctorProfileDto> GetDoctorProfileAsync(string doctorId)
    {
        return await _context.Doctor
            .Where(d => d.DoctorId == doctorId)
            .Select(d => new DoctorProfileDto
            {
                Name = d.FirstName + " " + d.LastName,
                DoctorImage = d.DoctorImage,
            })
            .FirstOrDefaultAsync();
    }
}