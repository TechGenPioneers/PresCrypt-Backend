using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices
{
    public interface IDoctorPatientService
    {
        Task<IEnumerable<DoctorPatientDto>> GetPatientDetailsAsync(string doctorId);
    }
}

