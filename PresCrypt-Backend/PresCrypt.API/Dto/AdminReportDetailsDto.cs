namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminReportDetailsDto
    {
        public AdminPatientDto SinglePatient { get; set; }
        public List<AdminPatientDto> PatientList { get; set; }
        public AdminDoctorDto SingleDoctor { get; set; }
        public List<AdminDoctorDto> DoctorList { get; set; }
        public List<AvailabilityDto> Availability { get; set; }
        public List<AdminAllAppointmentsDto> Appointments { get; set; }
        public AdminUserActivityDto UserActivity { get; set; } 
    }
}
