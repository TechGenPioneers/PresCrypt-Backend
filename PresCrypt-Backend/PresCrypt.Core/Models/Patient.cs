using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PresCrypt_Backend.PresCrypt.Core.Models

{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required string PatientId { get; set; } = string.Empty; // Primary Key

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        
        public string? Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [ForeignKey("Email")]
        public User User { get; set; }

        public string? BloodGroup { get; set; }

        
        public string? NIC { get; set; }

        [Required]
        public string Address { get; set; }    
         public byte[]? ProfileImage { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
       


        [Required]
        public string PasswordHash { get; set; }

        public string Status { get; set; }  

        public DateTime? LastLogin { get; set; } // Nullable in case they haven't logged in
        [Required]
        public string ContactNo { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<PatientNotifications> Notifications { get; set; }
    }
}
