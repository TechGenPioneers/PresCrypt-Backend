using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AuthServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
