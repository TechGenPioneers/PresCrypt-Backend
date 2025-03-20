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
        public string DoctorId { get; set; }  // Foreign Key referencing Doctor

        [ForeignKey("DoctorId")]  // Explicitly map DoctorId as Foreign Key
        public Doctor Doctor { get; set; }  // Navigation Property

        [Required]
        public DateOnly AvailableDate { get; set; }  // Stores only Date

        [Required]
        public TimeOnly AvailableTime { get; set; }  // Stores only Time
    }
}
