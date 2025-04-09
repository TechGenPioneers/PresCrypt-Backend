using System.ComponentModel.DataAnnotations;
namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class DoctorRegDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Specialization { get; set; }
        public string SLMCRegId { get; set; }
        public IFormFile SLMCIdImage { get; set; }
        public string NIC { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public double Charge { get; set; }
        public string ConfirmPassword { get; set; }
        public string hospitalSchedules { get; set; }



        //public List<HospitalScheduleDTO> hospitalSchedules { get; set; }
    }

   

    
}
