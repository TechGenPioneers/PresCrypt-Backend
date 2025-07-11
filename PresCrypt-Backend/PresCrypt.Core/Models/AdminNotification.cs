using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class AdminNotification
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("Doctor")]
        public string? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [ForeignKey("Patient")]
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey("DoctorRequest")]
        public string? RequestId { get; set; }
        public DoctorRequest? DoctorRequest { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
