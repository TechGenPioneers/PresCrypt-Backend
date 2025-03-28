using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Doctor
    {

        [Key]
        [Required]
        public string DoctorId { get; set; }  

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; }

        [Required]
        [MaxLength(50)]
        public string SLMCRegId { get; set; } 

        public byte[] SLMCIdPhoto { get; set; }

        public byte[] ProfilePhoto { get; set; }

        public byte[] IdPhoto { get; set; }

        [Required]
        public string NIC { get; set; }  

        [Required]
        public bool EmailVerified { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; } 

        [Required]
        [MaxLength(20)]
        public bool Status { get; set; }  

        public DateTime? LastLogin { get; set; }

        //Navigation Property
        public ICollection<Doctor_Availability> Availabilities { get; set; }
    }
}
