using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class DoctorRequest

    {
        [Key]
        [Required]
        public required string RequestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        [MaxLength(100)]

        public string Specialization { get; set; }

        [Required]
        [MaxLength(50)]

        public string SLMCRegId { get; set; }

        [Required]
        public byte[] SLMCIdImage { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 10)]
        public string NIC { get; set; }

        [Required]
        [Range(0, 100000)]
        public double Charge { get; set; }

        public string RequestStatus { get; set; }

        public bool EmailVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CheckedAt { get; set; }

        public string? Reason { get; set; }
        public ICollection<RequestAvailability> AvailabilityRequest { get; set; }
    }
}