using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminHospitalDto
    {
        public string? HospitalId { get; set; }

        public string HospitalName { get; set; }

        public string Number { get; set; }

        public double Charge { get; set; }

        public string Address { get; set; }

        public string City { get; set; }
    }
}
