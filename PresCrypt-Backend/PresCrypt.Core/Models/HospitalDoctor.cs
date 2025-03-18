using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class HospitalDoctor
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        // Foreign Key to the Hospital table
        [Required]
        public string HospitalId { get; set; }

        // Foreign Key to the Doctor table
        [Required]
        public string DoctorId { get; set; }

        // Navigation Property to the Hospital
        [ForeignKey("HospitalId")]
        public Hospital Hospital { get; set; }

        // Navigation Property to the Doctor
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }
    }
}
