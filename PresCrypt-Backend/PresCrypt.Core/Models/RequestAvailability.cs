using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class RequestAvailability

    {
        [Key]
        public string AvailabilityRequestId { get; set; }

        [Required]
        [ForeignKey("DoctorRequest")]
        public string DoctorRequestId { get; set; }

        [Required]
        public string AvailableDay { get; set; }  

        [Required]
        public TimeOnly AvailableStartTime { get; set; }  

        public TimeOnly AvailableEndTime { get; set; } 

        [Required]
        [ForeignKey("Hospital")]
        public string HospitalId { get; set; }  

        public Hospital Hospital { get; set; }  

        public DoctorRequest DoctorRequest { get; set; }

    }
}
