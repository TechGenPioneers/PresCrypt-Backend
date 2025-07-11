namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdmindashboardDto
    {
        public int PatientVisit { get; set; }
        public int Appointments { get; set; }
        public int Doctors { get; set; }
        public int Patients { get; set; }
        public AppointmentsOverTimeDto[] AppointmentsOverTime { get; set; }
    }
}
