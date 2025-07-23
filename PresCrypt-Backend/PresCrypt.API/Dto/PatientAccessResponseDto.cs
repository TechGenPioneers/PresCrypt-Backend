namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class PatientAccessResponseDto
    {
        public int NotificationId { get; set; }
        public string DoctorId { get; set; }
        public bool Accepted { get; set; }
    }
}
