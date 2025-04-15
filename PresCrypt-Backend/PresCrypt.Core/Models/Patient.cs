
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models

{
    public class Patient
    {
        
        public required string PatientId { get; set; } // Primary Key

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string BloodGroup { get; set; }

        [Required]
        public string NIC { get; set; }

        public byte[] ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; } 

        public DateTime UpdatedAt { get; set; }

        public string Status { get; set; }  // Example: "Active" or "Inactive"

        public DateTime? LastLogin { get; set; } // Nullable in case they haven't logged in
        [Required]
       
        public ICollection<Appointment> Appointments { get; set; }
    }
}

