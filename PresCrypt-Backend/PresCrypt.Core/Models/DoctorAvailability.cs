using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class DoctorAvailability
    {
        [Key]
        
        public int AvailabilityId { get; set; }

        [Required]
        public string DoctorId { get; set; }  // Foreign Key referencing Doctor

        [ForeignKey("DoctorId")]  // Explicitly map DoctorId as Foreign Key
        
        public string AvailableDay { get; internal set; }
        [Required]
        [ForeignKey("HospitalId")]
        public string HospitalId { get; set; }  // Foreign Key referencing Hospital


        [Required]
        public TimeOnly AvailableStartTime { get; set; }  // Stores only Time
        [Required]
        public TimeOnly AvailableEndTime { get; set; }

        public Doctor Doctor { get; set; }  // Navigation Property
        [Required]
        public Hospital Hospital { get; set; }  // Navigation Property

    }
}
