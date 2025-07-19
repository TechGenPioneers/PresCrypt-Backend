using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class DoctorPatientAccessRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [Required]
        public string PatientId { get; set; }

        public DateTime RequestDateTime { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Approved, Rejected
        public DateTime AccessExpiry { get; set; } = DateTime.Now.AddHours(1); // Default expiry time of 1 hour
        public DateTime GrantedAt { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
