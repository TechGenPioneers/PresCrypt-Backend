using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentSave
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string HospitalId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public double Charge { get; set; }
        public string Status { get; set; }
        public string TypeOfAppointment { get; set; }

        public string PaymentId { get; set; }
    }

}
