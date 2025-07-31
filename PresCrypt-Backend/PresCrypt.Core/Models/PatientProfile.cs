namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class PatientProfile
    {

        public Guid Id { get; set; }

        public string? OpenMrsId { get; set; }

        public required string Name { get; set; }

        public required string Gender { get; set; }

        public required string DOB { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public String? Address { get; set; }
    }
}
