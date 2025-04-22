namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminAllDoctorRequestDto
    {
        public string RequestId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Specialization { get; set; }

        public string CreatedAt { get; set; }

        public string Status { get; set; }

        public string CheckedAt { get; set; }
    }
}
