using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminReportController : ControllerBase
    {
        private readonly IAdminReportService _adminReportService;
        public AdminReportController(IAdminReportService adminReportService)
        {
            _adminReportService = adminReportService;
        }
        //get all details
        [HttpGet ("GetAll")]
        public async Task<IActionResult> GetAllDetails()
        {
            try
            {
                var Details = await _adminReportService.GetAllDetails();
                if (Details == null)
                {
                    return NotFound("No any Details Found");
                }
                return Ok(Details);
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
