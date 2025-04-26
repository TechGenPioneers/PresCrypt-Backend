using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityDto
    {
        public string? AvailabilityId { get; set; }

        public string Day { get; set; }

        public String? DoctorId { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string HospitalId { get; set; }

        public string HospitalName { get; set; }
    }
}