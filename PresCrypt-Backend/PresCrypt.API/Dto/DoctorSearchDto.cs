public class DoctorSearchDto
{
    public string DoctorName { get; set; }
    public List<string> AvailableDates { get; set; }
    public List<TimeSpan> AvailableTimes { get; set; } // TimeSpan instead of DateTime

    public byte[] Id { get; set; }  // Image stored as a byte array

}
