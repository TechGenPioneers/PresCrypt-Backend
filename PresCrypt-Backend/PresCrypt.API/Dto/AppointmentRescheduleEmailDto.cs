public class AppointmentRescheduleEmailDto
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string AppointmentId { get; set; }
    public DateTime NewDateTime { get; set; }
    public DateTime OldDateTime { get; set; }
}
