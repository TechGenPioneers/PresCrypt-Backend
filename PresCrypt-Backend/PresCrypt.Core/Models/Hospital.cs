using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Hospital
    {
        [Key]
        [Required]
        public string HospitalId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HospitalName { get; set; }  

        [Required]
        [MaxLength(15)]
        public string Number { get; set; }

        [Required]
        public double Charge { get; set; }

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }  

        [Required]
        [MaxLength(100)]
        public string City { get; set; }
        public ICollection<DoctorAvailability> DoctorAvailabilities { get; set; }

    }
}
