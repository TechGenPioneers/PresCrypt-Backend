using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{

    public class PatientNotificationHub : Hub
    {
        public async Task SendNotification(string patientId, string title, string message)
        {
            await Clients.User(patientId).SendAsync("ReceiveNotification", new { Title = title, Message = message });
        }
    }



     
}