namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentsOverTimeDto
    {
        public string Day { get; set; }
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Missed { get; set; }
    }
}
