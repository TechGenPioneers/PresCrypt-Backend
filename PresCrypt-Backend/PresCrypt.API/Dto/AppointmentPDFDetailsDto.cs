namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentPDFDetailsDto
    {
        public string PatientId { get; set; }
        public string DoctorName { get; set; }
        public string HospitalName { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public decimal TotalCharge { get; set; }
    }
}
