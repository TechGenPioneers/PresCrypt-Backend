public class DoctorSearchDto
{
    public string DoctorId { get; set; }
    public string DoctorName { get; set; }

    public double Charge { get; set; }
    public List<DateTime> AvailableDates { get; set; }
    public List<TimeSpan> AvailableTimes { get; set; } // TimeSpan instead of DateTime

    public byte[] Id { get; set; }  // Image stored as a byte array

}
