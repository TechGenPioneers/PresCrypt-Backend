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
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; }

        // Navigation property for Doctor
        public Doctor Doctor { get; set; }

        [Required]
        public string AvailableDay { get; set; }

        [Required]
        public TimeOnly AvailableStartTime { get; set; }

        [Required]
        public TimeOnly AvailableEndTime { get; set; }

        [Required]
        [ForeignKey("HospitalId")]
        public string HospitalId { get; set; }

        // Navigation property for Hospital
        public Hospital Hospital { get; set; }
    }
}
