namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentViewDialogDto
    {
        public string AppointmentId { get; set; }          
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string HospitalName { get; set; }
        public string AppointmentTime { get; set; }       
        public string Status { get; set; }
        public DateOnly AppointmentDate { get; set; }
    }
}
