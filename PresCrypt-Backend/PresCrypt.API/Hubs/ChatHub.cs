using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            // Try get user ID from claims
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Fallback to Context.UserIdentifier if available
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.UserIdentifier;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                // Add connection ID and user ID to online users
                OnlineUsers.TryAdd(Context.ConnectionId, userId);

                // Add connection to a group named by userId for private messaging
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);

                // Notify all clients about user connected
                await Clients.All.SendAsync("UserConnected", userId);

                // Send full list of online users to all clients
                var onlineUserIds = OnlineUsers.Values.Distinct().ToList();
                await Clients.All.SendAsync("GetOnlineUsers", onlineUserIds);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (OnlineUsers.TryRemove(Context.ConnectionId, out var userId))
            {
                // Remove connection from the user's group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

                // Notify all clients about user disconnected
                await Clients.All.SendAsync("UserDisconnected", userId);

                // Send updated list of online users
                var onlineUserIds = OnlineUsers.Values.Distinct().ToList();
                await Clients.All.SendAsync("GetOnlineUsers", onlineUserIds);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(
            string senderId,
            string senderType,
            string receiverId,
            string receiverType,
            string text,
            byte[]? image = null)
        {
            // Optional: Validate senderId matches the current user to prevent spoofing
            var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId != currentUserId)
            {
                throw new HubException("Unauthorized senderId");
            }

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = senderId,
                SenderType = senderType,
                ReceiverId = receiverId,
                ReceiverType = receiverType,
                Text = text,
                Image = image,
                SendAt = DateTime.UtcNow,
                IsReceived = false,
                IsRead = false
            };

            // Save message to DB
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Deliver message to receiver group
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.SenderId,
                message.SenderType,
                message.ReceiverId,
                message.ReceiverType,
                message.Text,
                message.Image,
                message.SendAt
            });

            // Mark message as received
            message.IsReceived = true;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();

            // Optional: send confirmation to sender
            await Clients.Caller.SendAsync("MessageSent", new
            {
                message.Id,
                message.IsReceived
            });
        }

        public async Task MarkAsRead(string messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();

                // Notify sender about message read
                await Clients.Group(message.SenderId).SendAsync("MessageRead", new
                {
                    messageId = message.Id,
                    readerId = message.ReceiverId,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
