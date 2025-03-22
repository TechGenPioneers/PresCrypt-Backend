namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentDisplayDto
    {
        public string AppointmentId { get; set; }

        public string PatientId { get; set; }

        public DateTime Time { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }
        
        public string PatientName { get; set; }
        
        public string DoctorName { get; set; }

        public string Gender { get; set; }

        public DateTime DOB { get; set; }

        public string ProfilePictureUrl { get; set; }
    }
}