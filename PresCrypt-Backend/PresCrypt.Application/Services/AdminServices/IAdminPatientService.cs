using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminPatientService
    {
        public Task<List<AdminAllPatientDto>> GetAllPatients();
        public Task<AdminPatientAppointmentsDto> GetPatientById(string patientId);
        public Task<string> UpdatePatient(AdminUpdatePatientDto updatePatient);
    }
}
