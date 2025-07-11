using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class DoctorNotificationHub : Hub
    {
        public async Task<object> JoinDoctorGroup(string doctorId)
        {
            var groupName = $"doctor-{doctorId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Use exact case matching
            await Clients.Caller.SendAsync("GroupJoined", groupName);

            return new
            {
                Success = true,
                Group = groupName,
                ConnectionId = Context.ConnectionId
            };
        }

        public async Task SendTestNotification(string doctorId)
        {
            var testNotification = new
            {
                Id = Guid.NewGuid().ToString(),
                Title = "System Test",
                Message = $"Test notification at {DateTime.UtcNow}",
                Type = "Test",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await Clients.Group($"doctor-{doctorId}")
                .SendAsync("ReceiveNotification", testNotification);
        }
    }
}