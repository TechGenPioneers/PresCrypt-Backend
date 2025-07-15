using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminContactUsService
    {
        public Task<List<AdminContactUsDto>> GetAllMessages();
        public Task<AdminContactUsDto> GetMessageById(string InquiryId);
        public Task ReadMsg(string InquiryId);

        public Task<string> SendReply(AdminContactUsDto adminContactUsDto);
    }
}
