namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ChatUsersDto
    {
        public string FullName { get; set; }
        public string ReceiverId { get; set; }
        public byte[] Image { get; set; }
        public string LastMessage { get; set; }
    }
}
