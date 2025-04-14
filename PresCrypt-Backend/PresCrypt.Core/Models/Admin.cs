using System.ComponentModel.DataAnnotations;
namespace PresCrypt_Backend.PresCrypt.Core.Models

{
    public class Admin
    {
        [Key]
        public string AdminId { get; set; } // Primary Key

        [Required]
        public string AdminName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; } // Example: "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string PasswordHash { get; set; }

        public string Status { get; set; } // Example: "Active" or "Inactive"

        public DateTime? LastLogin { get; set; } // Nullable if they haven't logged in yet


    }
}

