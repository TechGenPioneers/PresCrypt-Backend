using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorAvailabilityDto
    {
        public AdminDoctorDto Doctor { get; set; }
        public List<AvailabilityDto> Availability { get; set; }
    }
}
