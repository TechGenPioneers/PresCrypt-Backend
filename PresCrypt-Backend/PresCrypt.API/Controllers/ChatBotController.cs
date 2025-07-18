using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "Patient")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ChatController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "AIzaSyBeFcwffiHhmX_E1beXAzWoo6Jycru2QQU");
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            var payload = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[] {
                        new { text = request.Message }
                    }
                }
            }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent",
                content
            );

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }

}
