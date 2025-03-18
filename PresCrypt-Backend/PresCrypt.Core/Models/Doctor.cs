using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;

public class Doctor
{
    [Key]
    [Required]
    public string DoctorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DoctorName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; }

    [Required]
    [MaxLength(50)]
    public string SLMCRegId { get; set; }

    public byte[] Id { get; set; }

    [Required]
    public long NIC { get; set; }

    public string Description { get; set; }

    public double Charge { get; set; }

    [Required]
    public bool EmailVerified { get; set; }

    [Required]
    public string Role { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; }

    public DateTime? LastLogin { get; set; }

    // Navigation property for the HospitalDoctor relationship
    public ICollection<HospitalDoctor> HospitalDoctors { get; set; }
}
