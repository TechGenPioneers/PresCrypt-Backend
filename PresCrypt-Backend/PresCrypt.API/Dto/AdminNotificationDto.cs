using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminNotificationDto
    {
        public string? Id { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public string? AdminId { get; set; }
        public string? RequestId { get; set; }
        public string Type { get; set; }  // e.g., "Signup", "Request", "Alert"
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
