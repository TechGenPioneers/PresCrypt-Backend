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

        // 1. Get today's upcoming and cancelled appointment counts for the doctor
        var upcomingCount = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.Date == today && a.Status == "Pending" && a.Time > now);

        var cancelledCount = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.Date == today && a.Status == "Cancelled");

        // 2. Get all-time unique patients who had any appointment with this doctor
        var bookedPatientsCount = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        // 3. Get all hospitals where the doctor has had appointments historically
        var doctorHospitalList = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => new { a.HospitalId, a.Hospital.HospitalName })
            .Distinct()
            .ToListAsync();

        // 4. Get today’s pending appointments grouped by hospital
        var todayHospitalCounts = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.Date == today && a.Status == "Pending" && a.Time > now)
            .GroupBy(a => new { a.HospitalId, a.Hospital.HospitalName })
            .Select(g => new
            {
                g.Key.HospitalId,
                g.Key.HospitalName,
                Count = g.Count()
            })
            .ToListAsync();

        // 5. Merge to include hospitals with zero appointments
        var hospitalAppointmentCounts = doctorHospitalList
            .Select(h => new HospitalAppointmentDto
            {
                HospitalId = h.HospitalId,
                HospitalName = h.HospitalName,
                AppointmentCount = todayHospitalCounts.FirstOrDefault(x => x.HospitalId == h.HospitalId)?.Count ?? 0
            })
            .ToList();

        // 6. Return the final DTO
        return new DoctorDashboardDto
        {
            UpcomingAppointments = upcomingCount,
            CancelledAppointments = cancelledCount,
            BookedPatients = bookedPatientsCount,
            HospitalAppointments = hospitalAppointmentCounts
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