using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorRequestAvailabilityDto
    {
        public string AvailabilityRequestId { get; set; }
        public string DoctorRequestId { get; set; }
        public string AvailableDay { get; set; }
        public string AvailableStartTime { get; set; }
        public string AvailableEndTime { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
    }
}
