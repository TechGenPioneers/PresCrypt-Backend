using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityDto
    {
        public string day { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
    }
}
