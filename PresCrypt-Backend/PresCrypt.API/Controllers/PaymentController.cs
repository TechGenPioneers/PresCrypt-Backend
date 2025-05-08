using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.PaymentServices;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/Controller")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("payment/add")]
        
        public async Task<IActionResult> AddPayment([FromBody] Payment payment)
        {
            var result = await _paymentService.AddPaymentAsync(payment);
            return Ok(result);
        }
    }
}
