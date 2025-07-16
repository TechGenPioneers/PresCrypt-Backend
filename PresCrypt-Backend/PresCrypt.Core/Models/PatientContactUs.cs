using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PresCrypt_Backend.PresCrypt.Core.Models;

public class PatientContactUs
{
    [Key]
    [Required]
    [MaxLength(10)]
    public string InquiryId { get; set; } 

    [Required]
    public string PatientId { get; set; }  

    [ForeignKey("PatientId")]
    public Patient Patient { get; set; }  

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Topic { get; set; }

    [Required]
    public string Description { get; set; }

    public string ? ReplyMessage { get; set; }

    public bool? IsRead { get; set; } = false;

}
