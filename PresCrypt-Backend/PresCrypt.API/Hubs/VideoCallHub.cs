using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class VideoCallHub : Hub
    {
        public async Task NotifyPatient(string patientId, string doctorId, string roomUrl)
        {
            await Clients.User(patientId).SendAsync("IncomingCall", new
            {
                doctorId,
                roomUrl
            });
        }

        public async Task NotifyDoctorCallAccepted(string doctorId, string patientId)
        {
            await Clients.User(doctorId).SendAsync("CallAccepted", new
            {
                patientId
            });
        }
    }
}
