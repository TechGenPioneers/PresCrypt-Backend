namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorDashboardDto
    {
        public int UpcomingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int BookedPatients { get; set; }
        //public int TelehealthPatients { get; set; }
    }
}
