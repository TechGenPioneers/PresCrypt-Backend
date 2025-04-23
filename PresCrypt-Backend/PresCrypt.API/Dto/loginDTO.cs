using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
