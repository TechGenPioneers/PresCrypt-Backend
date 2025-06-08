using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.ChatServices
{
    public interface IChatServices
    {
        public Task SendMessage(ChatDto chatDto);
        public Task<List<ChatDto>> GetAllMessages(string senderId, string receiverId);
        public Task<ChatDto> GetLastMessage(string senderId, string receiverId);
        public Task MarkMessagesAsRead(string senderId, string receiverId);
        public Task<List<ChatUsersDto>> GetAllUsers(string senderId);
    }
}
