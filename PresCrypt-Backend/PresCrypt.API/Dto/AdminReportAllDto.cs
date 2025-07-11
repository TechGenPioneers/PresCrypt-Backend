namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminReportAllDto
    {
        public List<AdminReportDoctorsDto> Doctors { get; set; }
        public List<AdminReportPatientsDto> Patients { get; set; }
        public List<string> Specialty { get; set; }
    }
}
