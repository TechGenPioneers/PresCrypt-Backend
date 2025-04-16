using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Appointment
    {
        [Key]
        [Required]
        public string AppointmentId { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        [ForeignKey("Doctor")]  // ✅ Ensure it properly links to the Doctor table
        public string DoctorId { get; set; }

        public Doctor Doctor { get; set; }  // Navigation Property

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public byte[] SpecialNote { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeOfAppointment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
