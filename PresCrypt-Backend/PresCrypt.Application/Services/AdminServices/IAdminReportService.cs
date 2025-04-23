using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminReportService
    {
        public Task<AdminReportAllDto> GetAllDetails();
    }
}
