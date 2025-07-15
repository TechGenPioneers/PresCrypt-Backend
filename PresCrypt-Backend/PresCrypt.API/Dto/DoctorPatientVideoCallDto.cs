namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorPatientVideoCallDto
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DoctorPatientVideoCallDto(string firstName, string lastName)
        {
            FullName = $"{firstName} {lastName}";
        }
    }
}
