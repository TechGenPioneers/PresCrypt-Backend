using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ChatDto
    {
        public string? Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }
        public byte[]? Image { get; set; }
        public DateTime? SendAt { get; set; }
        public Boolean? IsReceived { get; set; }
        public Boolean? IsRead { get; set; }
    }
}
