using System.ComponentModel.DataAnnotations;
namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorRegDTO
    {
        [Required]
        [MaxLength(100)]
        public string DoctorName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; }

        [Required]
        [MaxLength(50)]
        
        public string SLMCRegId { get; set; } // Medical License Number Validation

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
        [Required]
        public string NIC { get; set; }


    }
}
