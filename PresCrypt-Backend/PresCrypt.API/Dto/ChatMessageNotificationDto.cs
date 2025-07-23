namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ChatMessageNotificationDto
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public string Type { get; set; }
    }
}
