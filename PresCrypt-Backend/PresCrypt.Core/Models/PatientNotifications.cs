using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class PatientNotifications
    {
            public string Id { get; set; }

            [ForeignKey("Patient")] 
            public string PatientId { get; set; }
            public Patient Patient { get; set; }
            public string Title { get; set; }    
            public string Message { get; set; }  
            public bool IsRead { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
