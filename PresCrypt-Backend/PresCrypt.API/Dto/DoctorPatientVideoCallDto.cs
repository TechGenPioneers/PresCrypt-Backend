namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorPatientVideoCallDto
    {
        public string fullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DoctorPatientVideoCallDto(string firstName, string lastName)
        {
            fullName = $"{firstName} {lastName}";
        }
    }
}
