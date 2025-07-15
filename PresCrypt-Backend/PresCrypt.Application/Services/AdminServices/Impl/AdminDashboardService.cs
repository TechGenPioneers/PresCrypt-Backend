using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AdminNotificationHub> _hubContext;

        public AdminDashboardService(ApplicationDbContext context, IHubContext<AdminNotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task<AdmindashboardDto> GetDashboardData(string userName)
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
                    Completed = g.Count(a => a.Status == "Completed"),
                    Missed = g.Count(a => a.Status == "Cancelled")
                })
                .OrderBy(a => a.Day)
                .ToArray();

            var user = await _context.Admin.FirstOrDefaultAsync(d => d.Email == userName);

            if(user == null)
            {
                throw new Exception("User not found");
            }

            dashboardData.AdminName = user.FirstName+" "+user.LastName;
            dashboardData.PatientVisit = await _context.Patient
                 .CountAsync(p => p.LastLogin.HasValue && p.LastLogin.Value.Date == DateTime.Today);

            dashboardData.Appointments = await _context.Appointments
                .CountAsync(a => a.Date == DateOnly.FromDateTime(DateTime.Today));

            dashboardData.Doctors = await _context.Doctor.CountAsync();
            dashboardData.Patients = await _context.Patient.CountAsync();

            return dashboardData;
        }

        public async Task CreateAndSendNotification(AdminNotificationDto adminNotification)
        {
            adminNotification.Id = Guid.NewGuid().ToString();
            adminNotification.CreatedAt = DateTime.UtcNow;
            adminNotification.IsRead = false;

            var notificationEntity = new AdminNotification
            {
                Id = adminNotification.Id,
                DoctorId = adminNotification.DoctorId,
                PatientId = adminNotification.PatientId,
                Type = adminNotification.Type,
                Title = adminNotification.Title,
                Message = adminNotification.Message,
                IsRead = adminNotification.IsRead,
                CreatedAt = adminNotification.CreatedAt.ToLocalTime()
            };

            _context.AdminNotifications.Add(notificationEntity);
            await _context.SaveChangesAsync();


            // Send to all connected clients (admins)
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Id = adminNotification.Id,
                Title = adminNotification.Title,
                Message = adminNotification.Message,
                Type = adminNotification.Type,
                CreatedAt = adminNotification.CreatedAt,
                DoctorId = adminNotification.DoctorId,
                PatientId = adminNotification.PatientId
            });
        }

        public async Task<List<AdminNotificationDto>> GetNotifications()
        {
            return await _context.AdminNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminNotificationDto
                {
                    Id = n.Id,
                    DoctorId = n.DoctorId,
                    PatientId = n.PatientId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<string> MarkNotificationAsRead(string notificationId)
        {
            var notification = await _context.AdminNotifications.FindAsync(notificationId);

            if (notification == null)
            {
                return "Notification not found";
            }

            notification.IsRead = true;

            var result = await _context.SaveChangesAsync();

            return result > 0 ? "Success" : "Error updating notification";
        }
        
        public async Task<string> MarkAllAsRead()
        {
            var unreadNotifications = await _context.AdminNotifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            var result = await _context.SaveChangesAsync();

            return result > 0 ? "Success" : "Error updating notifications";
        }
    }
}
