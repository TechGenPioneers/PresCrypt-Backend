namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class HospitalScheduleDTO
    {
        public string hospital { get; set; }
        public string hospitalId { get; set; }

        public Dictionary<string, AvailabilityTime> availability { get; set; }
    }
}
