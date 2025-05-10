using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PresCrypt_Backend.PresCrypt.Core.Models

{
    public class Admin
    {
        [Key]
        public string AdminId { get; set; } // Primary Key

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [ForeignKey("Email")]
        public User User { get; set; }

        [Required]
        public string Role { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string PasswordHash { get; set; }

        

        public DateTime? LastLogin { get; set; }


    }
}

