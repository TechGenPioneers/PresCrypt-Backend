using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using PresCrypt_Backend.Migrations;

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
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
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

        public async Task SendMessage(ChatDto chatDto)
        {
            if (chatDto == null || string.IsNullOrEmpty(chatDto.SenderId) || string.IsNullOrEmpty(chatDto.ReceiverId))
            {
                throw new ArgumentException("Invalid chat data.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = chatDto.SenderId,
                ReceiverId = chatDto.ReceiverId,
                Text = chatDto.Text,
                Image = chatDto.Image,
                SendAt = DateTime.Now,
                IsReceived = false,
                IsRead = false
            };

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                throw;
            }

            try
            {
                await Clients.Group(chatDto.ReceiverId).SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.SenderId,
                    message.ReceiverId,
                    message.Text,
                    message.Image,
                    SendAt = message.SendAt.ToString("o")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("SignalR send error: " + ex.Message);
                throw;
            }

            message.IsReceived = true;

            try
            {
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database update error: " + ex.Message);
            }

            try
            {
                await Clients.Caller.SendAsync("MessageSent", new
                {
                    message.Id,
                    message.IsReceived
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("SignalR confirmation send error: " + ex.Message);
            }
        }


        public async Task MarkAsRead(string senderId, string messageId)
        {
            // Notify sender group that this particular messageId has been read
            await Clients.Group(senderId).SendAsync("MessageRead", new
            {
                MessageIds = new List<string> { messageId }
            });
        }


        public async Task NotifyMessageDeleted(string userId, string messageId)
            {
                await Clients.Group(userId).SendAsync("MessageDeleted", messageId);
            }
        



    }
}
