namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminAllDoctorsDto
    {
        public string? DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Specialization{get; set;}
        public bool Status { get; set; }
        public byte[]? ProfilePhoto { get; set; }
    }
}
