using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminDoctorService
    {
       public Task<List<AdminDoctorDto>> GetAllDoctor();

        public Task<string> SaveDoctor(AdminDoctorDto dto);
    }
}
