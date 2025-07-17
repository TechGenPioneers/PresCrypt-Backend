namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AccessRequestResponseDto
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public bool Accepted { get; set; }
    }
}
