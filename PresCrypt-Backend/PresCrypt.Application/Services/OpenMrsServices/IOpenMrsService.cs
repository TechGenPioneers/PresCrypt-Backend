using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{
    public interface IOpenMrsService
    {
        Task<bool> CreateObservationsAsync(PatientVitalsDto vitals);
        Task<string> GetPatientOpenMrsIdAsync(string patientId);
    }
}
