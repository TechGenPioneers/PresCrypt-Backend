namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminPatientAppointmentsDto
    {
        public AdminPatientDto Patient { get; set; }
        public List<AdminAllAppointmentsDto> Appointments { get; set; }
    }
}
