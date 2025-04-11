namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentDisplayDto
    {
        public string AppointmentId { get; set; }

        public string PatientId { get; set; }

        public string HospitalId { get; set; }

        public string HospitalName { get; set; }

        public TimeOnly Time { get; set; }

        public DateOnly Date { get; set; }

        public string Status { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string PatientName { get; set; }

        public string Gender { get; set; }

        public DateTime DOB { get; set; }

        public byte[] ProfileImage { get; set; }
    }
}