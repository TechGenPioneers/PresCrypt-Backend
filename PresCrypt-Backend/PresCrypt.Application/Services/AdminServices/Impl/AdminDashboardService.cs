using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdmindashboardDto> GetDashboardData()
        {
            var dashboardData = new AdmindashboardDto();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = today.AddDays(-7);
            var endDate = today.AddDays(-1);

            dashboardData.AppointmentsOverTime = _context.Appointments
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .GroupBy(a => a.Date)
                .AsEnumerable() // <-- switch to LINQ-to-Objects here
                .Select(g => new AppointmentsOverTimeDto
                {
                    Day = g.Key.DayOfWeek.ToString().Substring(0, 3),
                    Total = g.Count(),
                    Completed = g.Count(a => a.Status == "Complete"),
                    Missed = g.Count(a => a.Status == "Missed")
                })
                .OrderBy(a => a.Day)
                .ToArray();




            dashboardData.PatientVisit = await _context.Patient
                 .CountAsync(p => p.LastLogin.HasValue && p.LastLogin.Value.Date == DateTime.Today);


            dashboardData.Appointments = await _context.Appointments
                .CountAsync(a => a.Date == DateOnly.FromDateTime(DateTime.Today));


            dashboardData.Doctors = await _context.Doctor.CountAsync();
            dashboardData.Patients = await _context.Patient.CountAsync();

            return dashboardData;
        }

    }
}
