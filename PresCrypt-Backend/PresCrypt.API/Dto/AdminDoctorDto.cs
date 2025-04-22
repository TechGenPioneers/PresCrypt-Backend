namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminDoctorDto
    {
        public string? RequestID { get; set; }
        public string? DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string Gender { get; set; }
        public double Charge { get; set; }
        public byte[]? ProfilePhoto { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string SlmcLicense { get; set; }
        public string NIC { get; set; }
        public string? Description { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? Status { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? LastLogin  { get; set; }
        public string ContactNumber { get; set; }
       

        public override string ToString()
        {
            return $"Doctor: {DoctorId} \n" +
                   $"Name: {FirstName} {LastName}, \n" +
                   $"Specialization: {Specialization}, \n" +
                   $"Nic: {NIC}, \n"+
                   $"SLMC License: {SlmcLicense},\n" +
                   $"Contact: {ContactNumber}, \n" +
                   $"Email: {Email}";
        }

    }
}
