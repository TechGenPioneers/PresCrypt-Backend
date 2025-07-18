using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientVideoServices;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Doctor,Patient")]
    public class DoctorPatientVideoCallController : ControllerBase
    {
        private readonly IVideoCallService _videoCallService;
        private readonly IHubContext<VideoCallHub> _hubContext;

        public DoctorPatientVideoCallController(IVideoCallService videoCallService, IHubContext<VideoCallHub> hubContext)
        {
            _videoCallService = videoCallService;
            _hubContext = hubContext;
        }

        [HttpPost("create-room")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request, [FromQuery] string patientId)
        {
            // Create Whereby room URL
            var roomUrl = await _videoCallService.CreateRoomAsync(request.RoomName);

            // Get the doctorId from the logged-in user's claims (adjust claim type as needed)
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "UnknownDoctor";

            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId query parameter is required to notify patient.");
            }

            // Notify the patient of the incoming call via SignalR
            await _hubContext.Clients.User(patientId).SendAsync("IncomingCall", new
            {
                doctorId,
                roomUrl
            });

            return Ok(new { roomUrl });
        }

        [HttpGet("user-names")]
        public async Task<IActionResult> GetUserNames([FromQuery] string doctorId, [FromQuery] string patientId)
        {
            try
            {
                var doctorName = await _videoCallService.GetDoctorNameAsync(doctorId);
                var patientName = await _videoCallService.GetPatientNameAsync(patientId);

                return Ok(new
                {
                    DoctorName = doctorName,
                    PatientName = patientName
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching user names");
            }
        }


        [HttpGet("get-room/{roomId}")]
        public IActionResult GetRoom(string roomId)
        {
            return BadRequest("Getting room by ID is not supported by Whereby API.");
        }

        [HttpGet("access-url/{roomId}")]
        public async Task<IActionResult> GetAccessUrl(string roomId, [FromQuery] string role)
        {
            var url = await _videoCallService.GenerateAccessUrlAsync(roomId, role);
            return Ok(new { url });
        }
    }
}
