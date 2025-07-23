namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentCancellationEmailDto
    {

            public string Email { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly AppointmentTime { get; set; }
  

    }
}
