using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorAvailabilityDto
    {
        public DoctorDto doctor { get; set; }
        public List<AvailabilityDto> availability { get; set; }
    }
}
