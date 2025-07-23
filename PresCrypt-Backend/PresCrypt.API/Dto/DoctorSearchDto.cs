public class DoctorSearchDto
{

    public string DoctorId { get; set; }

    public string HospitalId { get; set; }
    public string FirstName { get; set; }

    public string LastName { get; set; }
    public double Charge { get; set; }// Hospital Charge

    public string Specialization { get; set; }

    public string HospitalName { get; set; }


    public List<String> AvailableDay { get; set; }
    public List<TimeSpan> AvailableTime { get; set; } // TimeSpan instead of DateTime

    public byte[] Image { get; set; }  

}