
using System.ComponentModel.DataAnnotations;
namespace PresCrypt_Backend.PresCrypt.API.Dto
{


    public class PatientRegDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Status { get; set; }

        [Required]
        [Phone]
        public string ContactNumber { get; set; }
        [Required]
        public string NIC { get; set; }

        public string BloodGroup { get; set; }
    }
}
