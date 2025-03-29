namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorBookingDto
    {
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }

        public double Charge { get; set; }//this is for Doctor Charge
        public string Description { get; set; }
        public byte[] Id { get; set; }

    }
}
