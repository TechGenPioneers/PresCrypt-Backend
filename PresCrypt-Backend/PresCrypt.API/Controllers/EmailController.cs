using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class EmailController : ControllerBase
    {
        private readonly IAdminEmailService _adminEmailService;
        public EmailController(IAdminEmailService adminEmailService)
        {
            _adminEmailService = adminEmailService;
        }


        [HttpPost]
        public async Task<IActionResult> AdminSendEmail([FromBody] AdminEmailDto adminEmailDto)
        {
            if (adminEmailDto == null)
            {
                BadRequest("Empty");
            }

           var sent = await _adminEmailService.SendEmail(adminEmailDto);
            if(sent == "Success")
            {
                return Ok(sent);

            } else
            {
                return BadRequest(sent);
            }
           
        }
    }
}
