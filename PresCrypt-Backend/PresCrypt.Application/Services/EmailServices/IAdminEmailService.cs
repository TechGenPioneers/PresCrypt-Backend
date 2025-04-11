using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices
{
    public interface IAdminEmailService
    {
        Task<string> SendEmail(AdminEmailDto adminEmailDto);
    }
}
