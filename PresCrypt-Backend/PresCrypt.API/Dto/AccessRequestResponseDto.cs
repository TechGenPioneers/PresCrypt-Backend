using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AccessRequestResponseDto
    {
        [Required]
        public string PatientId { get; set; }
        [Required]
        public string DoctorId { get; set; }
        public bool Accepted { get; set; }
    }
}
