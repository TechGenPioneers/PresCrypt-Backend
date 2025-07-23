using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string AppointmentId { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [Required]
        [ForeignKey(nameof(Hospital))]
        public string HospitalId { get; set; }
        public Hospital Hospital { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }

        [Required]
        public double Charge { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public string? SpecialNote { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeOfAppointment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ForeignKey attribute removed from PaymentId
        public string? PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}
