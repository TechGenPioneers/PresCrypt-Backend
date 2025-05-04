public class DoctorReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Patient { get; set; }  // "all" or specific patient ID
    public string ReportType { get; set; }
    public string DoctorId { get; set; }
}
