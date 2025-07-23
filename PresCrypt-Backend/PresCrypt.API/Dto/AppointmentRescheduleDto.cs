public class AppointmentRescheduleDto
{
    public string DoctorId { get; set; }
    public string HospitalId { get; set; }
    public string HospitalName { get; set; }
    public List<string> AppointmentIds { get; set; }
}
