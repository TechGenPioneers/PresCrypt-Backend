using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Appointment
{
    [Key]
    [Required]
    public string AppointmentId { get; set; }  // Primary Key

    [Required]
    [ForeignKey(nameof(Patient))]  // Foreign Key referencing Patient.UserId
    public string PatientId { get; set; }

    public Patient Patient { get; set; }  // Navigation Property for Patient


        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public string DoctorId { get; set; }  // Foreign Key referencing Doctor table


    public Doctor Doctor { get; set; }  // Navigation Property for Doctor (not a foreign key)

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly Time { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; }

    public string SpecialNote { get; set; }

    [Required]
    [MaxLength(50)]
    public string TypeOfAppointment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
