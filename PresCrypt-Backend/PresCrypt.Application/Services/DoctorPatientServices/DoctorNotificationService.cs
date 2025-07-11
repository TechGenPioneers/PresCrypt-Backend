using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.API.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices
{
    public class DoctorNotificationService : IDoctorNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DoctorNotificationHub> _hubContext;

        public DoctorNotificationService(
            ApplicationDbContext context,
            IHubContext<DoctorNotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateAndSendNotificationAsync(
            string doctorId,
            string message,
            string title,
            string type)
        {
            // Create notification in database first
            var notification = new DoctorNotification
            {
                DoctorId = doctorId,
                Message = message,
                Title = title,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification to doctor's group
            try
            {
                var groupName = $"doctor-{doctorId}";
                Console.WriteLine($"Attempting to send notification to group: {groupName}");

                var notificationDto = new DoctorNotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };

                Console.WriteLine($"Notification payload: {System.Text.Json.JsonSerializer.Serialize(notificationDto)}");

                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notificationDto);

                Console.WriteLine($"Notification sent to group: {groupName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<DoctorNotificationDto>> GetNotificationsForDoctorAsync(string doctorId)
        {
            return await _context.DoctorNotifications
                .Where(n => n.DoctorId == doctorId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new DoctorNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            var notification = await _context.DoctorNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            var notification = await _context.DoctorNotifications.FindAsync(notificationId);
            if (notification == null)
            {
                return false;
            }

            _context.DoctorNotifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}