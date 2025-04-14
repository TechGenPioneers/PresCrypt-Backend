namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminAllAppointmentsDto
    {
        public string AppointmentId { get; set; }
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double Charge { get; set; }
        public string Status { get; set; }
        public string? SpecialNote { get; set; }
        public string TypeOfAppointment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }

    }
}
