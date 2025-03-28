using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Doctor_Availability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvailabilityId { get; set; }

        [Required]
        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }

        [Required]
        public DateOnly AvailableDay { get; set; }

        [Required]
        public TimeOnly AvailableStartTime { get; set; }

        [Required]
        public TimeOnly AvailableEndTime { get; set; }

        [Required]
        [ForeignKey("HospitalId")]
        public string HospitalId { get; set; }
    }
}
