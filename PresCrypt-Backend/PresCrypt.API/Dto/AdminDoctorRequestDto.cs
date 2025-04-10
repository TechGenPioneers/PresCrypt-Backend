using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminDoctorRequestDto
    {
        public string RequestId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }

        public string ContactNumber { get; set; }

        public string Specialization { get; set; }

        public string SLMCRegId { get; set; }

        public byte[] SLMCIdImage { get; set; }

        public string NIC { get; set; }

        public double Charge { get; set; }

        public string RequestStatus { get; set; }

        public bool EmailVerified { get; set; }

        public string CreatedAt { get; set; }

        public string CheckedAt { get; set; }

    }
}
