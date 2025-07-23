using Microsoft.Extensions.Primitives;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminUserActivityDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string LastLogin { get; set; }
        public string LastAppointmentCreatedAt { get; set; }
        public string LastAppointmentStatus { get; set; }

    }
}
