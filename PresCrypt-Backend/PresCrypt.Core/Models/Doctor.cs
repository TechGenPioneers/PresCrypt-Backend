using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.ComponentModel.DataAnnotations.Schema; 


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
    public string Gender { get; set; }

    public byte[] DoctorImage { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [ForeignKey("Email")]
    public User User { get; set; }

    [Required]
    [Phone]
    public string ContactNumber { get; set; }

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
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public double TotalAmtToPay { get; set; }

    [Required]
    public bool Status { get; set; }

    public DateTime? LastLogin { get; set; }


    // Navigation Property
    public ICollection<DoctorAvailability> Availabilities { get; set; }
    public ICollection<Appointment> Appointments { get; set; }  // One doctor can have multiple appointments

}