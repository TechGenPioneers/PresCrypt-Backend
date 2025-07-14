using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.ChatServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMsgController : ControllerBase
    {
        private readonly IChatServices _chatServices;
        public ChatMsgController(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages(string senderId, string receiverId)
        {
            try
            {
                var messages = await _chatServices.GetAllMessages(senderId, receiverId);
                if (messages == null || !messages.Any())
                {
                    return NotFound("No messages found.");
                }
                return Ok(messages);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving messages: {e.Message}");
            }
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
        {
            try
            {
                if (chatDto == null)
                {
                    return BadRequest("Chat data cannot be null.");
                }
               await _chatServices.SendMessage(chatDto);
                return Ok("Message sent successfully.");
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while sending the message: {e.Message}");
            }
        }
        [HttpPatch("MarkMessagesAsRead")]
        public async Task<IActionResult> MarkMessagesAsRead(string senderId, string receiverId)
        {
            try
            {
                await _chatServices.MarkMessagesAsRead(senderId, receiverId);
                return Ok("Messages marked as read successfully.");
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while marking messages as read: {e.Message}");
            }
        }

        [HttpGet("GetChatUsers")]
        public async Task<ActionResult<List<ChatUsersDto>>> GetChatUssersq(string senderId)
        {
            try
            {
                var users = await _chatServices.GetAllUsers(senderId);

                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                return Ok(users);
            }
            catch (Exception e)
            {
                return BadRequest($"An error occurred while retrieving doctors: {e.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            if(messageId == null)
            {
                return BadRequest();
            }
            await _chatServices.DeleteMessage(messageId);
            return Ok("Deleted");
        }
    }
}