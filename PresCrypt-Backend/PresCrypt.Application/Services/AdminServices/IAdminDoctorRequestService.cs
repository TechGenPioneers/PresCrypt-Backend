using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminDoctorRequestService
    {
        public Task<List<AdminAllDoctorRequestDto>> GetAllDoctorRequest();
        public Task<RequestDoctorAvailabilityDto> getRequestByID(string requestID);
        public Task<string> RejectRequest(DoctorRequestRejectDto rejected);
        public Task<string> ApprovRequest(string requestId);
    }
}
