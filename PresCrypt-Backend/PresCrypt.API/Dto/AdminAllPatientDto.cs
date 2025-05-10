namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminAllPatientDto
    {
        public string PatientId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DOB { get; set; }

        public string Gender { get; set; }

        public string LastAppointmentDoctorName {get;set;}

        public string LastAppointmentDoctorID { get; set; }

        public string LastAppointmentDate { get; set; }

        public string Status { get; set; }

        public byte[] ProfileImage { get; set; }

        public string LastLogin { get; set; }
    }
}
