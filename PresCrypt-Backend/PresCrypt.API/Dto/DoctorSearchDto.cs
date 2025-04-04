public class DoctorSearchDto
{

    public string DoctorId { get; set; }
    public string FirstName { get; set; }

    public double Charge { get; set; }// this is for Hospital Charge
    public List<String> AvailableDay { get; set; }
    public List<TimeSpan> AvailableTime { get; set; } // TimeSpan instead of DateTime

    public byte[] Id { get; set; }  // Image stored as a byte array

}
