using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class UserRegDTO
    {
            
        public string UserName { get; set; }
        
       
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
       

        public string Role { get; set; }
    }
}
