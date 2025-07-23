using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PaymentServices
{
    public class PaymentService :IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService (ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddPaymentAsync (Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }
}
