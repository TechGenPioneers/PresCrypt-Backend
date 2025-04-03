using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class Doctor
{
    [Key]
    [Required]
    public string DoctorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName{ get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [Required]
    public char Gender { get; set; }

    public byte[] DoctorImage { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string ContactNo { get; set; } 

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; }

    [Required]
    [MaxLength(50)]
    public string SLMCRegId { get; set; }

    public byte[] SLMCIdImage { get; set; }

    [Required]
    public string NIC { get; set; }

    public string Description { get; set; }

    public double Charge { get; set; }

    [Required]
    public bool EmailVerified { get; set; }


    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }


    [Required]
    [MaxLength(20)]
    public bool Status { get; set; }


    // Navigation Property
    public ICollection<DoctorAvailability> Availabilities { get; set; }
    

   
}
