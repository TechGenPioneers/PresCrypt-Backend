namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ChatUsersDto
    {
        public string FullName { get; set; }
        public string LastMessageSenderId { get; set; }
        public string ReceiverId { get; set; }
        public byte[] Image { get; set; }
        public byte[] ProfileImage { get; set; }
        public DateTime SendAt { get; set; }
        public string LastMessage { get; set; }
        public Boolean IsRead { get; set; }
        public Boolean IsReceived { get; set; }
    }
}
