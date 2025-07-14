using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class VideoCallHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        // Doctor notifies patient about incoming call
        public async Task NotifyPatient(string patientId, string doctorId, string roomUrl, string doctorName)
        {
            await Clients.User(patientId).SendAsync("IncomingCall", new
            {
                doctorId,
                doctorName, // Pass doctor's name
                roomUrl
            });
        }

        // Patient notifies doctor call accepted
        public async Task NotifyDoctorCallAccepted(string doctorId, string patientId)
        {
            await Clients.User(doctorId).SendAsync("CallAccepted", new 
            {
                patientId = patientId
            });
        }

        //NEW: Patient notifies doctor call rejected
        public async Task NotifyDoctorCallRejected(string doctorId, string patientId)
        {
            await Clients.User(doctorId).SendAsync("CallRejected", new
            {
                patientId = patientId
            });
        }
    }
}