using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminDoctorService
    {
        public Task<List<HospitalDto>> getAllHospitals();
        public Task<List<AdminAllDoctorsDto>> GetAllDoctor();
        public Task<string> SaveDoctor(DoctorAvailabilityDto dto);
        public Task<DoctorAvailabilityDto> getDoctorById(string doctorID);
        
    }
}
