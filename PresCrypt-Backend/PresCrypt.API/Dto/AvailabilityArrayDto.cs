namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityArrayDto
    {
        public string? DoctorID { get; set; }
        public List<AvailabilityDto> Availability { get; set; }  // Changed to list of AvailabilitySlot objects
    }
}
