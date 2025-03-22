using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class DoctorAvailability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvailabilityId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; }  // Foreign Key referencing Doctor

        [Required]
        public string HospitalId { get; set; }  // Foreign Key referencing Hospital

        [Required]
        public DateOnly AvailableDate { get; set; }  // Stores only Date

        [Required]
        public TimeOnly AvailableTime { get; set; }  // Stores only Time

        // Navigation Properties
        public Doctor Doctor { get; set; }
        public Hospital Hospital { get; set; }
    }
}
