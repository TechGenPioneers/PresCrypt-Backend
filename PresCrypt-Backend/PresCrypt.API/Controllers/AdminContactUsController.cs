using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminContactUsController : ControllerBase
    {
        private readonly IAdminContactUsService _adminContactUsService;
        public AdminContactUsController(IAdminContactUsService adminContactUsService)
        {
            _adminContactUsService = adminContactUsService;
        }

        //get all Messages details
        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var Messages = await _adminContactUsService.GetAllMessages();

                if (Messages == null || !Messages.Any())
                {
                    return NotFound("No Messages found.");
                }

                return Ok(Messages);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving Messages: {e.Message}");
            }
        }

        [HttpGet("{InquiryId}")]
        public async Task<IActionResult> GetMessageById(string InquiryId)
        {
            try
            {
                var Message = await _adminContactUsService.GetMessageById(InquiryId);

                if (Message == null)
                {
                    return NotFound("No Messages found.");
                }

                return Ok(Message);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving Messages: {e.Message}");
            }
        }

        [HttpPatch("{InquiryId}")]
        public async Task<IActionResult> ReadMsg(string InquiryId)
        {
            try
            {
                await _adminContactUsService.ReadMsg(InquiryId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while marking the message as read: {e.Message}");
            }
        }

        [HttpPost("SendReply")]
        public async Task<IActionResult> SendReply([FromBody] AdminContactUsDto adminContactUsDto)
        {
            try
            {
                var result = await _adminContactUsService.SendReply(adminContactUsDto);
                return Ok(result); // Return the message (e.g., "Reply sent successfully.")
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while sending the reply: {e.Message}");
            }
        }


    }
}
