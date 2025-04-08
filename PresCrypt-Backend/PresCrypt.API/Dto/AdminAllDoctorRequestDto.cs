namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminAllDoctorRequestDto
    {
        public string RequestId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Specialization { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
