using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorAvailabilityDto
    {
        public DoctorDto Doctor { get; set; }
        public List<AvailabilityDto> Availability { get; set; }
    }
}
