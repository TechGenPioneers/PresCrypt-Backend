namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }  // Reset token sent via email
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
