namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityDisplayDto
    {
        public string AvailabilityId { get; set; }
        public string DoctorId { get; set; }
        public string AvailableDay { get; set; }
        public TimeOnly AvailableStartTime { get; set; }
        public TimeOnly AvailableEndTime { get; set; }
    }
}