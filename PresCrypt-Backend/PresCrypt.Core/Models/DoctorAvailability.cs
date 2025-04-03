using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class DoctorAvailability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string AvailabilityId { get; set; } 

        [Required]
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; }  // Foreign Key referencing Doctor

        [Required]
        public string AvailableDay { get; set; }  // Stores only Day

        public string HospitalId { get; set; }  // Foreign Key referencing Hospital

        [Required]
        public TimeOnly AvailableStartTime { get; set; }  // Stores only Time

        public TimeOnly AvailableEndTime { get; set; }


        // Navigation Properties
        public Doctor Doctor { get; set; }
        public Hospital Hospital { get; set; }

    }
}
