namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string email { get; set; }
        public string specialization { get; set; }
        public string slmcLicense { get; set; }
        public string contactNumber { get; set; }
        public string hospital { get; set; }

        public override string ToString()
        {
            return $"Doctor: {FirstName}" + $" {LastName}, \n" +
                   $"Specialization: {specialization}, \n" +
                   $"SLMC License: {slmcLicense},\n" +
                   $" Contact: {contactNumber}, \n" +
                   $"Hospital: {hospital}, \n" +
                   $"Email: {email}";
        }

    }
}
