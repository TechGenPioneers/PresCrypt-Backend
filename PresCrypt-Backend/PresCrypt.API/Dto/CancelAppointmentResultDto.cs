namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class CancelAppointmentResultDto
    {
        public bool Success { get; set; }
        public string? PaymentMethod { get; set; }
        public DateOnly? AppointmentDate { get; set; }
        public TimeOnly? AppointmentTime { get; set; }
    }
}
