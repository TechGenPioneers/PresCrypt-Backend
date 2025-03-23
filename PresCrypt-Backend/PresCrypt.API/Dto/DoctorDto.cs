namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorDto
    {
        public string? DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? DoctorImage { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string SlmcLicense { get; set; }
        public string? Description { get; set; }
        public bool? EmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? status { get; set; }
        public DateTime? LastLogin  { get; set; }
        public string ContactNumber { get; set; }
        public string Nic { get; set; }

        public override string ToString()
        {
            return $"Doctor: {DoctorId} \n" +
                   $"Name: {FirstName} {LastName}, \n" +
                   $"Specialization: {Specialization}, \n" +
                   $"Nic: {Nic}, \n"+
                   $"SLMC License: {SlmcLicense},\n" +
                   $"Contact: {ContactNumber}, \n" +
                   $"Email: {Email}";
        }

    }
}
