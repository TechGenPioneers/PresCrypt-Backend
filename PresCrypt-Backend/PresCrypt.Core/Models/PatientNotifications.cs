using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class PatientNotifications
    {

            [Key]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [ForeignKey("Patient")] 
            public string PatientId { get; set; }
            public Patient Patient { get; set; }

            [ForeignKey("Doctor")]
            public string? DoctorId { get; set; } // Nullable in case the notification isn't doctor-related
            public Doctor Doctor { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }    
            public string Message { get; set; }  
            public bool IsRead { get; set; }
            public bool? IsResponded { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
