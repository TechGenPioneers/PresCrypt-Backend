public class DoctorDashboardDto
{
    public int UpcomingAppointments { get; set; } 
    public int CancelledAppointments { get; set; } 
    public int BookedPatients { get; set; } 
    public List<HospitalAppointmentDto> HospitalAppointments { get; set; } = new(); 
}
