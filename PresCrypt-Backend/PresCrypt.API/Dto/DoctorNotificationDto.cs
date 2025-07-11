namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorNotificationDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
