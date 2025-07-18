using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class VideoCallHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"Connection {Context.ConnectionId} left group {userId}");
        }

        public async Task AcceptCall(string doctorId, string patientId, string roomUrl)
        {
            // Use Groups instead of Clients.User unless you have auth setup
            await Clients.Group(doctorId).SendAsync("CallAccepted", new
            {
                roomUrl,
                patientId
            });
        }

        public async Task RejectCall(string doctorId, string patientId)
        {
            await Clients.Group(doctorId).SendAsync("CallRejected", new
            {
                patientId
            });
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            Console.WriteLine($"User {userId} connected to VideoCallHub with connection ID {Context.ConnectionId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            Console.WriteLine($"User {userId} disconnected from VideoCallHub with connection ID {Context.ConnectionId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
