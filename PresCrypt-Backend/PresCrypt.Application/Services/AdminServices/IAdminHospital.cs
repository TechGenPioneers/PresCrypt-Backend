using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminHospital
    {
        Task<AdminHospitalDto> AddHospital(AdminHospitalDto hospitalDto);
        Task<List<AdminHospitalDto>> GetAllHospital();
        Task<AdminHospitalDto> GetHospitalById(string hospitalId);
        Task<bool> UpdateHospital(AdminHospitalDto hospitalDto);
        Task<bool> DeleteHospital(string hospitalId);

    }
}
