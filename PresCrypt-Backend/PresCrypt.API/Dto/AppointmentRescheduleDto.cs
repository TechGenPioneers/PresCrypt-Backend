public class AppointmentRescheduleDto
{
    public string DoctorId { get; set; }
    public string HospitalId { get; set; }
    public string HospitalName { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
}
