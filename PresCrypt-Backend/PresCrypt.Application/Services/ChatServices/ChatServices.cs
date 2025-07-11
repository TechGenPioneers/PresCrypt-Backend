using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Generic;

namespace PresCrypt_Backend.PresCrypt.Application.Services.ChatServices
{
    public class ChatServices : IChatServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatServices(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task<List<ChatDto>> GetAllMessages(string senderId, string receiverId)
        {
            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.SendAt)
                .Select(m => new ChatDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Text = m.Text,
                    Image = m.Image,
                    SendAt = m.SendAt,
                    IsReceived = m.IsReceived,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return messages;
        }



        public async Task SendMessage(ChatDto chatDto)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = chatDto.SenderId,
                ReceiverId = chatDto.ReceiverId,
                Text = chatDto.Text,
                Image = chatDto.Image,
                SendAt = DateTime.Now,   // Better to use UTC time
                IsReceived = true,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Notify sender and receiver groups about the new message
            await _hubContext.Clients.Group(message.SenderId.ToString())
                .SendAsync("SendMessage", message);

            await _hubContext.Clients.Group(message.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", message);
        }


        public async Task<ChatDto> GetLastMessage(string senderId, string receiverId)
        {
            var message = await _context.Messages
                .Where(m =>
                    m.SenderId == senderId &&
                    m.ReceiverId == receiverId )
                .OrderByDescending(m => m.SendAt)
                .Select(m => new ChatDto
                {
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Text = m.Text,
                    Image = m.Image,
                    SendAt = m.SendAt,
                    IsReceived = m.IsReceived,
                    IsRead = m.IsRead
                })
                .FirstOrDefaultAsync();

            return message;
        }

        public async Task MarkMessagesAsRead(string senderId, string receiverId)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == receiverId && m.ReceiverId == senderId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                var readMessageIds = unreadMessages.Select(m => m.Id).ToList();

                // Notify receiver (the user who received the messages) about which messages were marked read
                await _hubContext.Clients.Group(receiverId)
                    .SendAsync("MessageRead", new
                    {
                        SenderId = senderId,
                        MessageIds = readMessageIds
                    });
            }
        }



        public async Task<List<ChatUsersDto>> GetAllUsers(string senderId)
        {
            var chatUsers = new List<ChatUsersDto>();

            if (senderId.StartsWith("D"))
            {
                var patientIds = await _context.Appointments
                .Where(a => a.DoctorId == senderId)
                .Select(a => a.PatientId)
                .Distinct()
                .ToListAsync();

               

                foreach (var patientId in patientIds)
                {
                    var patient = await _context.Patient
                        .Where(d => d.PatientId == patientId)
                        .Select(d => new ChatUsersDto
                        {
                            ReceiverId = d.PatientId,
                            FullName = d.FirstName + " " + d.LastName,
                            Image = d.ProfileImage
                        })
                        .FirstOrDefaultAsync();

                    if (patient != null)
                    {
                        var lastMessage = await _context.Messages
                            .Where(m =>
                                (m.SenderId == patientId && m.ReceiverId == senderId) ||
                                (m.SenderId == senderId && m.ReceiverId == patientId))
                            .OrderByDescending(m => m.SendAt)
                            .Select(m => new
                            {
                                m.Text,
                                m.IsRead,
                                m.SenderId,
                                m.SendAt,
                                m.IsReceived,
                                m.Image
                            })
                            .FirstOrDefaultAsync();

                        if (lastMessage != null)
                        {
                            patient.LastMessage = lastMessage.Text;
                            patient.Image = lastMessage.Image;
                            patient.IsRead = lastMessage.IsRead;
                            patient.LastMessageSenderId=lastMessage.SenderId;
                            patient.SendAt=lastMessage.SendAt;
                            patient.IsReceived=lastMessage.IsReceived;
                        }

                        chatUsers.Add(patient);
                    }

                }

            }
            else if (senderId.StartsWith("P"))
            {
                var doctorIds = await _context.Appointments
               .Where(a => a.PatientId == senderId)
               .Select(a => a.DoctorId)
               .Distinct()
               .ToListAsync();
                

                foreach (var doctorId in doctorIds)
                {
                    var doctor = await _context.Doctor
                        .Where(d => d.DoctorId == doctorId)
                        .Select(d => new ChatUsersDto
                        {
                            ReceiverId = d.DoctorId,
                            FullName = "Dr. "+d.FirstName + " " + d.LastName,
                            Image = d.DoctorImage
                        })
                        .FirstOrDefaultAsync();

                    if (doctor != null)
                    {
                        var lastMessage = await _context.Messages
                            .Where(m =>
                                (m.SenderId == doctorId && m.ReceiverId == senderId) ||
                                (m.SenderId == senderId && m.ReceiverId == doctorId))
                            .OrderByDescending(m => m.SendAt)
                            .Select(m => new
                            {
                                m.Text,
                                m.IsRead,
                                m.SenderId,
                                m.SendAt
                            })
                            .FirstOrDefaultAsync();

                        if (lastMessage != null)
                        {
                            doctor.LastMessage = lastMessage.Text;
                            doctor.IsRead = lastMessage.IsRead;
                            doctor.LastMessageSenderId=lastMessage.SenderId;
                            doctor.SendAt=lastMessage.SendAt;
                        }

                        chatUsers.Add(doctor);
                    }

                }

            }

            
            return chatUsers;
        }

        public async Task DeleteMessage(string messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }

            // Notify both sender and receiver
            await _hubContext.Clients.Group(message.SenderId.ToString())
                .SendAsync("MessageDeleted", messageId);
            await _hubContext.Clients.Group(message.ReceiverId.ToString())
                .SendAsync("MessageDeleted", messageId);
        }


    }
}
