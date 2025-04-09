using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Doctor
    {

        [Key]
        [Required]
        public string DoctorId { get; set; }  // Primary Key

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [ForeignKey("Email")]
        public User User { get; set; }

        [Required]
        [Phone]
        public string ContactNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; }

        [Required]
        [MaxLength(50)]
        public string SLMCRegId { get; set; }  

        public byte[] SLMCIdImage { get; set; }  // Image stored as a byte array

        [Required]
        public string NIC { get; set; }  // Assuming NIC as a numeric value (long)

        [Required]
        public bool EmailVerified { get; set; }

        public string Gender { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current UTC time

        public DateTime? UpdatedAt { get; set; }  // Nullable, since it may not always be updated

        [Required]
        public string PasswordHash { get; set; }  // Storing hashed password
        //public string ConfirmPassword { get; set; }  

        [Required]
        
        public bool Status { get; set; }  // Active, Inactive, etc.

        public DateTime? LastLogin { get; set; }  // Nullable, as it may not always be available

        // Navigation Property
        public ICollection<DoctorAvailability> Availabilities { get; set; }
        
    }
}
