namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityDisplayDto
    {
        public int AvailabilityId { get; set; }
        public string DoctorId { get; set; }
        public string AvailableDay { get; set; }
        public TimeOnly AvailableTime { get; set; }
    }
}