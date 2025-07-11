public class AppointmentRescheduleResultDto
{
    public bool Success { get; set; }
    public string OriginalAppointmentId { get; set; }
    public string NewAppointmentId { get; set; }
    public DateTime? NewDateTime { get; set; }
    public string Message { get; set; }
}