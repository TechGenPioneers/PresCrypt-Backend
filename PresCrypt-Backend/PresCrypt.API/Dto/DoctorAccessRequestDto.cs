namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorAccessRequestDto
    {
        public string DoctorId { get; set; }
        public string PatientId { get; set; }
        public string Title { get; set; }  // for notification
        public string Message { get; set; }
    }
}
