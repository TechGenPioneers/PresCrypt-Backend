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
                Console.WriteLine($"User {userId} joined group with connection {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error joining group: {ex.Message}");
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
            try
            {
                Console.WriteLine($"Patient {patientId} accepted call from doctor {doctorId}");

                // Notify the doctor that call was accepted
                await Clients.Group(doctorId).SendAsync("CallAccepted", new
                {
                    roomUrl,
                    patientId,
                    acceptedAt = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting call: {ex.Message}");
                throw;
            }
        }

        public async Task RejectCall(string doctorId, string patientId)
        {
            try
            {
                Console.WriteLine($"Patient {patientId} rejected call from doctor {doctorId}");

                // Notify the doctor that call was rejected
                await Clients.Group(doctorId).SendAsync("CallRejected", new
                {
                    rejectedBy = patientId,
                    message = $"Call rejected by patient {patientId}",
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting call: {ex.Message}");
                throw;
            }
        }

        public async Task CancelCall(string patientId, string doctorId)
        {
            try
            {
                Console.WriteLine($"Doctor {doctorId} cancelled call to patient {patientId}");

                // Notify the patient that call was cancelled
                await Clients.Group(patientId).SendAsync("CallCancelled", new
                {
                    cancelledBy = doctorId,
                    message = $"Call cancelled by doctor",
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling call: {ex.Message}");
                throw;
            }
        }

        public async Task NotifyIncomingCall(string receiverId, string callerId, string callerName, string roomUrl)
        {
            try
            {
                Console.WriteLine($"Notifying {receiverId} of incoming call from {callerId}");

                // Notify the receiver group that they have an incoming call
                await Clients.Group(receiverId).SendAsync("IncomingCall", new
                {
                    callerId,
                    callerName,
                    roomUrl,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying incoming call: {ex.Message}");
                throw;
            }
        }

        // Method to check if a user is online
        public async Task CheckUserOnlineStatus(string userId)
        {
            try
            {
                // You can implement logic to check if user is connected
                // For now, we'll just return a simple response
                await Clients.Caller.SendAsync("UserOnlineStatus", new
                {
                    userId,
                    isOnline = true, // You can implement actual logic here
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user status: {ex.Message}");
                throw;
            }
        }

        // Method to send typing indicators (optional)
        public async Task SendTypingIndicator(string receiverId, string senderId, bool isTyping)
        {
            try
            {
                await Clients.Group(receiverId).SendAsync("TypingIndicator", new
                {
                    senderId,
                    isTyping,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending typing indicator: {ex.Message}");
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.GetHttpContext().Request.Query["userId"];
                Console.WriteLine($"User {userId} connected to VideoCallHub with connection ID {Context.ConnectionId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);

                    // Optional: Notify other users that this user is online
                    await Clients.Others.SendAsync("UserConnected", new
                    {
                        userId,
                        connectionId = Context.ConnectionId,
                        timestamp = DateTime.UtcNow.ToString("o")
                    });
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.GetHttpContext().Request.Query["userId"];
                Console.WriteLine($"User {userId} disconnected from VideoCallHub with connection ID {Context.ConnectionId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

                    // Optional: Notify other users that this user went offline
                    await Clients.Others.SendAsync("UserDisconnected", new
                    {
                        userId,
                        connectionId = Context.ConnectionId,
                        timestamp = DateTime.UtcNow.ToString("o")
                    });
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }
        }
    }
}