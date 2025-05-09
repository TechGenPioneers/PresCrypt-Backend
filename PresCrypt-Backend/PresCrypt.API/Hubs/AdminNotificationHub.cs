using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class AdminNotificationHub : Hub
    {
        public async Task SendNotification(AdminNotificationDto adminNotification)
        {
            await Clients.All.SendAsync("ReceiveNotification", adminNotification);
        }
    }
}
