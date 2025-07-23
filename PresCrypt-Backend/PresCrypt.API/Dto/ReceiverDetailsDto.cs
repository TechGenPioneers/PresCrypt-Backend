namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ReceiverDetailsDto
    {
        public string Gender { get; set; }
        public string Specialization { get; set; }
        public string Description { get; set; }
        public bool DoctorStatus { get; set; }
        public string PatientStatus { get; set; }
        public string PatientPhoneNumber { get; set; }
        public DateOnly LastAppointmentDate { get; set; }
        public string LastAppointmentStatus {get;set;}
    }
}
