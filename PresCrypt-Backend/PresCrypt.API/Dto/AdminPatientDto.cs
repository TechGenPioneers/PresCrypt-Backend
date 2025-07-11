using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminPatientDto
    {
        public string PatientId { get; set; } 

        public  string FirstName { get; set; }

        public  string LastName { get; set; }

        public string DOB { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }

        public string NIC { get; set; }

        public byte[] ProfileImage { get; set; }

        public string CreatedAt { get; set; }

        public string UpdatedAt { get; set; }

        public string Status { get; set; }

        public string? LastLogin { get; set; } 
    }
}
