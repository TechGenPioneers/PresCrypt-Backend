using System.Globalization;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentCountDto
    {

        public string DoctorId { get; set; }
        public List<DateTime> Dates { get; set; }


    }
}
