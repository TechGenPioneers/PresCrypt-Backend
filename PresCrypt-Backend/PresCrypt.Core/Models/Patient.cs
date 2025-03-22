using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Patient
    {
        [Key]
        [Required]
        public string PatientId { get; set; }  // Primary Key

        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; }

        [Required]
        public DateTime DOB { get; set; }  // Date of Birth

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(12)]
        public string NIC { get; set; }

        public string BloodGroup { get; set; }

        //public byte[] ProfilePicture { get; set; }  // Store as binary (optional)

        [Required]
        public string Role { get; set; }

        [Required]
        public string PasswordHash { get; set; }  // Storing hashed password

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current UTC time

        public DateTime? UpdatedAt { get; set; }  // Nullable, since it may not always be updated

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }  // Active, Inactive, etc.

        public DateTime? LastLogin { get; set; }  // Nullable, as it may not always be available

        // Relationship with Appointments
        public ICollection<Appointment> Appointments { get; set; }
    }
}
