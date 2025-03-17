namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorDto
    {
        public string? doctorId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string specialization { get; set; }
        public string slmcLicense { get; set; }
        public string contactNumber { get; set; }
        public string [] hospital { get; set; }

        public override string ToString()
        {
            return $"Doctor: {doctorId} \n" +
                   $"Name: {firstName} {lastName}, \n" +
                   $"Specialization: {specialization}, \n" +
                   $"SLMC License: {slmcLicense},\n" +
                   $"Contact: {contactNumber}, \n" +
                   $"Hospital: {string.Join(", ", hospital)}, \n" + // Use string.Join() to display array values
                   $"Email: {email}";
        }

    }
}
