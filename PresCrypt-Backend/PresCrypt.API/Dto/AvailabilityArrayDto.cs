namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AvailabilityArrayDto
    {
        public string? doctorID { get; set; }
        public List<AvailabilityDto> availability { get; set; }  // Changed to list of AvailabilitySlot objects
    }
}
