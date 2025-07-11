public class RescheduleRequest
{
    public string OriginalAppointmentId { get; set; }
    public string NewHospitalId { get; set; }
    public DateOnly NewDate { get; set; }
    public TimeOnly NewTime { get; set; }
}