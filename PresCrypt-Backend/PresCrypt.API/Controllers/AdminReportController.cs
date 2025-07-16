using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminReportController : ControllerBase
    {
        private readonly IAdminReportService _adminReportService;
        public AdminReportController(IAdminReportService adminReportService)
        {
            _adminReportService = adminReportService;
        }

        //get all details (pateint names , doctor names, Specializations)
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

        //get filtered details for admin request
        [HttpPost]
        public async Task<IActionResult> GetFilteredDetails([FromBody] AdminGetReportDetailsDto reportDetails)
        {
            try
            {
                var filteredDetails = await _adminReportService.GetFilteredDetails(reportDetails);
                if(filteredDetails == null)
                {
                    return NotFound("Not Fonud");
                }

                return Ok(filteredDetails);
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
