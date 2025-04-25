namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class RequestDoctorAvailabilityDto
    {
        public AdminDoctorRequestDto Request { get; set; }
        public List<DoctorRequestAvailabilityDto> RequestAvailability { get; set; }
    }
}
