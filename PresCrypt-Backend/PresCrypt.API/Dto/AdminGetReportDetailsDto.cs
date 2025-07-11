namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminGetReportDetailsDto
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Doctor { get; set; }
        public string? Patient { get; set; }
        public string? Specialty { get; set; }
        public string ReportType { get; set; }
    }
}
