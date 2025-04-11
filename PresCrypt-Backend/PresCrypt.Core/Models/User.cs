
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required string UserId { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        public required string Role { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpire { get; set; }

        public bool EmailVerified { get; set; } = false;

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LastFailedLoginTime { get; set; }
        public required ICollection<Patient> Patient { get; set; }
        public required ICollection<Doctor> Doctor { get; set; }
        public required ICollection<Admin> Admin { get; set; }
    }

}
