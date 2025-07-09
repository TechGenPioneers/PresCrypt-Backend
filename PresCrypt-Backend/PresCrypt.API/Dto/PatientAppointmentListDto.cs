using System.Globalization;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class PatientAppointmentListDto
    {
        public string AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string PatientEmail { get; set; }
        public string DoctorName { get; set; }
        public string DoctorEmail { get; set; }
        public string Specialization { get; set; }

        public string HospitalName { get; set; }

        public TimeOnly Time { get; set; }

        public DateOnly Date { get; set; }

        public string Status { get; set; }
    }
}
