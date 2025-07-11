using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PaymentServices
{
    public interface IPaymentService
    {
        Task<Payment> AddPaymentAsync(Payment payment);
    }
}
