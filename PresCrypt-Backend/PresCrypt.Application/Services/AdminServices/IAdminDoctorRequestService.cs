using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminDoctorRequestService
    {
        public Task<List<AdminAllDoctorRequestDto>> GetAllPendingDoctors();
        public Task<List<AdminAllDoctorRequestDto>> GetAllApprovedDoctors();
        public Task<List<AdminAllDoctorRequestDto>> GetAllRejectedDoctors();
    }
}
